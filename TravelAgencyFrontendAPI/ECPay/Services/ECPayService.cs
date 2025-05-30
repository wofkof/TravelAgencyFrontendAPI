// 路徑：TravelAgencyFrontendAPI/ECPay/Services/ECPayService.cs


using System.Security.Cryptography; // 用於 SHA256 (金流用) 和 AES (發票用)
using System.Text;
using System.Text.Encodings.Web;    
using System.Text.Json;             
using System.Threading.Tasks;
using System.Web; // 用於 HttpUtility
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;    // 用於 FindAsync 和 SaveChangesAsync
using Microsoft.Extensions.Configuration; // 用於讀取 HostURL
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;       // 用於 IOptions
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;     
using TravelAgencyFrontendAPI.ECPay.Models;

namespace TravelAgencyFrontendAPI.ECPay.Services
{
    public class ECPayService
    {
        private readonly ECPayConfiguration _ecpayConfig;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ECPayService> _logger;
        private readonly string _hostUrl; // 後端 API 部署網址
        private readonly IHttpClientFactory _httpClientFactory;

        // --- 輔助類別：用於解析綠界發票 API (JSON+AES) 的回應 ---
        private class ECPayInvoiceResponseOuter
        {
            public JsonElement PlatformID { get; set; } // 使用 JsonElement
            public JsonElement MerchantID { get; set; } // 使用 JsonElement
            public ECPayRpHeader RpHeader { get; set; }
            public int? TransCode { get; set; }
            public string TransMsg { get; set; }
            public string Data { get; set; }

            public string GetPlatformIDAsString() => PlatformID.ValueKind == JsonValueKind.Number ? PlatformID.GetInt32().ToString() : PlatformID.ToString();
            public string GetMerchantIDAsString() => MerchantID.ValueKind == JsonValueKind.Number ? MerchantID.GetInt32().ToString() : MerchantID.ToString();
        }


        private class ECPayRpHeader
        {
            public long? Timestamp { get; set; }
        }

        private class ECPayInvoiceResponseInner
        {
            public int? RtnCode { get; set; }
            public string RtnMsg { get; set; }
            public string InvoiceNo { get; set; } // 發票號碼
            public string InvoiceDate { get; set; } // 發票開立時間
            public string RandomNumber { get; set; } // 隨機碼
        }
        // --- END 輔助類別 ---

        public class ECPayPaymentRequestViewModel // 金流用 ViewModel
        {
            public string EcPayAioCheckOutUrl { get; set; } // 綠界結帳頁面網址
            public Dictionary<string, string> Parameters { get; set; } // 送往綠界的參數
        }

        public ECPayService(IOptions<ECPayConfiguration> ecpayConfig, AppDbContext dbContext, IConfiguration configuration, ILogger<ECPayService> logger, IHttpClientFactory httpClientFactory)
        {
            _ecpayConfig = ecpayConfig.Value;
            _dbContext = dbContext;
            _logger = logger;
            _hostUrl = configuration.GetValue<string>("HostURL") ?? throw new ArgumentNullException("HostURL is not configured in appSettings.json");
            _httpClientFactory = httpClientFactory;

        }


        /// <summary>
        /// 生成綠界支付所需的參數物件 (不再是 HTML 表單)
        /// </summary>
        /// <param name="orderId">您的訂單 ID</param>
        /// <returns>包含綠界支付網址和參數的 ViewModel</returns>
        public async Task<ECPayPaymentRequestViewModel> GenerateEcPayPaymentForm(int orderId)
        {
            var order = await _dbContext.Orders
                                        .Include(o => o.OrderDetails)
                                        .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                _logger.LogWarning($"OrderId: {orderId} 的訂單未找到。"); // 原有備註
                throw new ArgumentException($"找不到訂單 ID 為 {orderId} 的訂單。");
            }

            // << 修改點：使用您更新的 OrderStatus >>
            if (order.Status != OrderStatus.Awaiting)
            {
                _logger.LogWarning($"[Payment] OrderId: {orderId} 的訂單狀態 ({order.Status}) 不允許支付。"); // 原有備註 + 更新
                throw new InvalidOperationException($"訂單 ID 為 {orderId} 的訂單狀態不允許支付。");
            }

            // --- 特店交易編號 (MerchantTradeNo) 生成邏輯 ---
            string merchantTradeNo = order.MerchantTradeNo;
            if (string.IsNullOrEmpty(merchantTradeNo))
            {
                var orderIdPart = order.OrderId.ToString("D5"); // 補零以增加穩定性 // 原有備註
                string prefix = "TRV"; // 您可以自訂前綴
                string timePart = DateTime.Now.ToString("yyMMddHHmmss"); // 縮短時間戳部分 // 原有備註
                string baseMtn = $"{prefix}{orderIdPart}{timePart}"; // 重新組合 // 原有備註
                merchantTradeNo = new string(baseMtn.Where(char.IsLetterOrDigit).ToArray()); // 確保只含英數字
                if (merchantTradeNo.Length > 20) // 確保長度不超過20碼
                {
                    merchantTradeNo = merchantTradeNo.Substring(0, 20);
                }
                _logger.LogInformation($"為訂單 ID: {order.OrderId} 生成/確認 MerchantTradeNo: {merchantTradeNo}"); // 原有備註
                order.MerchantTradeNo = merchantTradeNo;
                // 注意：此處的 SaveChangesAsync 若與 OrderController 的邏輯衝突，需調整。 // 原有備註
                // 最好是由 OrderController 負責 MerchantTradeNo 的最終確定和儲存。 // 原有備註
                // await _dbContext.SaveChangesAsync(); // 暫時註解，建議由 Controller 統一處理 SaveChanges // 原有備註
            }

            // --- 商品名稱 (ItemName) 和 交易描述 (TradeDesc) for Payment API ---
            // 綠界金流API的 ItemName 是將所有品名用 # 串接
            var itemName = string.Join("#", order.OrderDetails.Select(od => $"{SanitizeInvoiceItemName(od.Description ?? "商品")} x {od.Quantity}"));
            if (string.IsNullOrEmpty(itemName)) itemName = "旅遊行程商品"; // 原有備註
            if (itemName.Length > 200) itemName = itemName.Substring(0, 197) + "..."; // 原有備註

            var tradeDesc = $"旅遊訂單號: {order.OrderId}"; // 原有備註
            if (!string.IsNullOrEmpty(order.Note) && (tradeDesc.Length + order.Note.Length + 3) < 200)
            {
                tradeDesc += $" ({order.Note})";
            }
            if (tradeDesc.Length > 200) tradeDesc = tradeDesc.Substring(0, 197) + "..."; // 原有備註

            // << 修改點：TotalAmount for Payment >>
            // 您已確認 Order.TotalAmount 是含稅的，ECPay金流的 TotalAmount 應為顧客實際支付的含稅金額。
            string paymentTotalAmountString = ((int)order.TotalAmount).ToString(); // 直接使用含稅總額並轉為整數

            // --- 準備送往綠界的參數 ---
            var orderParams = new SortedDictionary<string, string>(StringComparer.Ordinal) // 使用 SortedDictionary 並指定 StringComparer.Ordinal 以確保參數名按字母順序排序（區分大小寫） // 原有備註
            {
                { "MerchantID", _ecpayConfig.MerchantID },          // 金流用 MerchantID
                { "MerchantTradeNo", merchantTradeNo },
                { "TotalAmount", paymentTotalAmountString },        // 金流的總金額 (整數)
                { "CustomField1", order.OrderId.ToString()},       // 訂單 ID
                { "CustomField2", order.OrdererEmail ?? "" },      // 訂單者 Email
                { "CustomField3", order.OrdererPhone ?? "" },      // 訂單者電話
                { "CustomField4", "" },                            // 自訂欄位4 
                { "MerchantTradeDate", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") },
                { "PaymentType", "aio" },
                { "ChoosePayment", "Credit" },                     // 預設信用卡
                { "TradeDesc", tradeDesc },
                { "ItemName", itemName },                          // 金流用的組合品名
                { "ReturnURL", $"{_hostUrl.TrimEnd('/')}/api/ECPay/ECPayReturn" },         // 伺服器端背景通知URL
                { "OrderResultURL", $"{_hostUrl.TrimEnd('/')}/api/ECPay/ECPayOrderResult" }, // 使用者端前景通知URL
                { "EncryptType", "1" },                            // 使用 SHA256 加密
            };

            // 計算 CheckMacValue (金流用)
            var checkMacValue = GetCheckMacValue(orderParams, _ecpayConfig.HashKey, _ecpayConfig.HashIV);
            orderParams.Add("CheckMacValue", checkMacValue);

            _logger.LogInformation($"已為訂單 ID: {orderId}, 綠界特店交易編號: {merchantTradeNo} 生成 ECPay 付款參數物件。"); // 原有備註

            return new ECPayPaymentRequestViewModel
            {
                EcPayAioCheckOutUrl = _ecpayConfig.ECPayAioCheckOutUrl, // 綠界金流介接的目標 URL // 原有備註
                Parameters = orderParams.ToDictionary(pair => pair.Key, pair => pair.Value) // 轉換為一般字典給前端 // 原有備註
            };
        }

        /// <summary>
        /// 呼叫綠界 API 開立電子發票 (使用 JSON + AES 加密)
        /// </summary>
        /// <param name="order">包含發票所需資訊的訂單物件</param>
        /// <param name="ecpayTradeNo">綠界支付成功後回傳的交易編號 (TradeNo) - 在此API中可能非主要用途，但可供記錄</param>
        /// <returns>從綠界 API 收到的回應參數字典</returns>
        public async Task<Dictionary<string, string>> IssueInvoiceAsync(Order order, string ecpayTradeNo)
        {
            if (order == null)
            {
                _logger.LogError("[Invoice] IssueInvoiceAsync: 訂單物件為 null。"); // 原有備註
                throw new ArgumentNullException(nameof(order), "訂單物件不可為 null 以進行發票開立。");
            }

            // 再次確認訂單明細已載入 (如果 order 物件可能未包含)
            if (order.OrderDetails == null || !order.OrderDetails.Any())
            {
                _logger.LogInformation($"[Invoice] IssueInvoiceAsync: 訂單 {order.OrderId} 的 OrderDetails 未載入或為空，嘗試重新讀取。"); // 原有備註
                var orderFromDb = await _dbContext.Orders.Include(o => o.OrderDetails)
                                        .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);
                if (orderFromDb == null || orderFromDb.OrderDetails == null || !orderFromDb.OrderDetails.Any())
                {
                    _logger.LogError($"[Invoice] IssueInvoiceAsync: 訂單 {order.OrderId} 的訂單明細為空或載入失敗。"); // 原有備註
                    throw new InvalidOperationException("開立發票需要訂單明細。");
                }
                order = orderFromDb; // 更新為從資料庫完整載入的訂單 // 原有備註
            }
            _logger.LogInformation($"[Invoice] 準備為訂單 {order.OrderId} (MTN: {order.MerchantTradeNo}) 開立發票。"); // 原有備註

            // Step 1: 準備內部 Data JSON 物件的參數 (根據綠界 /B2CInvoice/Issue 文件)
            var invoiceDataContent = new Dictionary<string, object>
            {
                // 使用發票專用的 MerchantID >>
                ["MerchantID"] = _ecpayConfig.Invoice_MerchantID,
                ["RelateNumber"] = order.MerchantTradeNo, // 特店自訂編號 (您的商店交易編號) // 原有備註 (部分)
                // ["CustomerID"] = order.MemberId.ToString(), // 可選，客戶編號 // 原有備註
            };

            // --- Customer Information & Invoice Options (使用預設值策略) ---
            invoiceDataContent["CustomerIdentifier"] = order.InvoiceOption == InvoiceOption.Company ? order.InvoiceUniformNumber ?? "" : ""; // 原有備註 (部分)

            if (order.InvoiceOption == InvoiceOption.Company)
            {
                invoiceDataContent["CustomerName"] = !string.IsNullOrWhiteSpace(order.InvoiceTitle) ? order.InvoiceTitle : order.OrdererName; // 原有備註 (部分)
            }
            else
            { // Personal
                invoiceDataContent["CustomerName"] = order.OrdererName; // 原有備註 (部分)
            }

            // << 修改點：明確使用預設值 for Print, Donation, Carrier >>
            // B2C 電子發票預設：不列印、不捐贈、發票存綠界(Email通知)
            string printFlag = "0"; // 是否列印：0=不列印 (B2C電子發票) // 原有備註
            // 根據綠界文件：當統一編號[CustomerIdentifier]有值時，且載具類別[CarrierType]為空值時，此參數請帶1 (Print="1")
            // 您選擇暫不修改Order模型，因此這裡的CarrierType會是預設的空字串。
            if (order.InvoiceOption == InvoiceOption.Company && !string.IsNullOrWhiteSpace(order.InvoiceUniformNumber) /* && string.IsNullOrEmpty(預設的carrierType空字串) */)
            {
                // printFlag = "1"; // 若要嚴格符合「公司戶+無載具 => 必列印」規則，可啟用此行。
                // 但啟用後，CustomerAddr 會變成必填，需要確保 order.InvoiceBillingAddress 有值。
                // 考量到您選擇暫不修改 Order 模型以加入明確的 PrintPreference，
                // 維持 printFlag = "0" (不列印) 可能是目前較簡單的做法，優先以電子化處理。
                // 如果綠界API因此報錯，則需要回頭調整此處邏輯或補齊 CustomerAddr。
                _logger.LogInformation($"[Invoice] 訂單 {order.OrderId}: 公司戶 ({order.InvoiceUniformNumber}) 且預設無載具。依綠界文件 Print 可能需為 '1'。目前設定為 '{printFlag}'。");
            }
            invoiceDataContent["Print"] = printFlag;

            // CustomerAddr: 當列印註記[Print]=1(列印)時，此參數為必填
            if (printFlag == "1")
            {
                if (string.IsNullOrWhiteSpace(invoiceDataContent["CustomerName"]?.ToString())) invoiceDataContent["CustomerName"] = "客戶"; // 補救措施
                invoiceDataContent["CustomerAddr"] = !string.IsNullOrWhiteSpace(order.InvoiceBillingAddress) ? order.InvoiceBillingAddress : "客戶地址"; // 補救措施
            }
            else
            { // 不列印時
                // 若使用者勾選加入帳單地址，或為公司戶且有帳單地址，則填入
                if ((order.InvoiceAddBillingAddr || order.InvoiceOption == InvoiceOption.Company) && !string.IsNullOrWhiteSpace(order.InvoiceBillingAddress))
                {
                    invoiceDataContent["CustomerAddr"] = order.InvoiceBillingAddress; // 原有備註 (部分)
                }
            }

            invoiceDataContent["CustomerPhone"] = order.OrdererPhone ?? ""; // 原有備註 (部分)
            // **重要**：電子發票寄送Email // 原有備註
            invoiceDataContent["CustomerEmail"] = !string.IsNullOrWhiteSpace(order.InvoiceDeliveryEmail) ? order.InvoiceDeliveryEmail : order.OrdererEmail;

            // 根據綠界文件，CustomerEmail 和 CustomerPhone 至少一個必填
            if (string.IsNullOrEmpty(invoiceDataContent["CustomerEmail"]?.ToString()) && string.IsNullOrEmpty(invoiceDataContent["CustomerPhone"]?.ToString()))
            {
                _logger.LogWarning($"[Invoice] 訂單 {order.OrderId}: CustomerEmail 和 CustomerPhone 皆為空，可能導致API錯誤或通知失敗。");
                // 雖然API文件說擇一，但強烈建議Email要有值以利電子發票寄送
            }

            invoiceDataContent["Donation"] = "0";   // 是否捐贈：0=不捐贈 (若要支援捐贈，此處需調整) // 原有備註
            invoiceDataContent["LoveCode"] = "";     // 愛心碼：若捐贈則必填 // 原有備註
            invoiceDataContent["CarrierType"] = "";  // 預設由 Email 傳送或存於綠界會員載具 // 原有備註 (部分)
            invoiceDataContent["CarrierNum"] = "";   // 若 CarrierType 為手機條碼("3")或自然人憑證條碼("2")時，此欄位必填 // 原有備註

            invoiceDataContent["TaxType"] = "1"; // 課稅類別：1=應稅 (大部分情況) // 原有備註
            // << 修改點：因為 Price 和 TotalAmount 皆為含稅，所以 vat="1" >>
            invoiceDataContent["vat"] = "1";   // 商品單價是否含稅：1=含稅 // 原有備註 (部分)

            // --- 金額計算 (您已確認 Order.TotalAmount 和 od.Price 均為含稅) ---
            var itemsForInvoice = new List<Dictionary<string, object>>();
            int itemSeq = 1;
            // List<decimal> itemAmountsFromDetailsForSum = new List<decimal>(); // 用於驗證加總

            foreach (var od in order.OrderDetails)
            {
                decimal itemCount = od.Quantity;
                decimal itemPriceAlreadyTaxed = od.Price;         // 直接使用，因為它是含稅單價
                decimal itemAmountAlreadyTaxed = od.TotalAmount;  // 直接使用，因為它是含稅小計

                itemsForInvoice.Add(new Dictionary<string, object>
                {
                    { "ItemSeq", itemSeq++ },
                    { "ItemName", SanitizeInvoiceItemName(od.Description ?? "商品") }, // 原有備註 (部分)
                    { "ItemCount", itemCount },
                    { "ItemWord", "式" },      // 品項單位，例如：套、組、式、個 // 原有備註 (部分)
                    { "ItemPrice", (int)Math.Round(itemPriceAlreadyTaxed, MidpointRounding.AwayFromZero) },  // 含稅單價
                    { "ItemAmount",(int)Math.Round(itemAmountAlreadyTaxed, MidpointRounding.AwayFromZero) }, // 含稅小計
                    // { "ItemRemark", od.Note ?? "" } // 商品備註 (可選，也需清理) // 原有備註
                });
                // itemAmountsFromDetailsForSum.Add(itemAmountAlreadyTaxed);
            }
            invoiceDataContent["Items"] = itemsForInvoice; // **發票品項 (Item Properties)** - 極重要！ // 原有備註

            // SalesAmount: 使用訂單的總含稅金額，並轉為整數
            // 綠界文件："所有商品的ItemAmount加總後四捨五入=SalesAmount(含稅)"
            // 您應確保 Order.TotalAmount 是 OrderDetails 中所有 TotalAmount 加總的結果 (考慮浮點數精度)
            invoiceDataContent["SalesAmount"] = (int)Math.Round(order.TotalAmount, MidpointRounding.AwayFromZero); // 銷售額總計 (含稅) // 原有備註 (部分)


            invoiceDataContent["InvoiceRemark"] = $"訂單編號: {order.MerchantTradeNo}"; // 發票備註 (可選) // 原有備註
            invoiceDataContent["InvType"] = "07"; // 發票字軌類別：07=一般稅額計算之電子發票 // 原有備註

            // --- Step 2-6: 序列化, URL編碼, AES加密, 組裝外部JSON, 發送請求 ---
            var jsonOptions = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            string jsonDataParametersString = JsonSerializer.Serialize(invoiceDataContent, jsonOptions);
            _logger.LogInformation($"[Invoice] 內部 Data JSON (序列化後): {jsonDataParametersString}");

            string urlEncodedDataRaw = HttpUtility.UrlEncode(jsonDataParametersString);
            string urlEncodedData = System.Text.RegularExpressions.Regex.Replace(urlEncodedDataRaw, "%[0-9a-f]{2}", m => m.Value.ToUpperInvariant());
            _logger.LogInformation($"[Invoice] URL 編碼後的 Data (強制大寫): {urlEncodedData}");
            // 然後用這個 urlEncodedData 進行 AES 加密

            string encryptedData;
            try
            {
                encryptedData = AESEncrypt(urlEncodedData, _ecpayConfig.Invoice_HashKey, _ecpayConfig.Invoice_HashIV); // << 使用發票專用 Key/IV >>
                _logger.LogInformation($"[Invoice] AES 加密後的 Data (Base64): {encryptedData}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Invoice] AES 加密 Data 失敗。");
                return new Dictionary<string, string> { { "RtnCode", "AES_ENCRYPT_ERROR" }, { "RtnMsg", ex.Message } };
            }

            var requestPayloadOuter = new
            {
                MerchantID = _ecpayConfig.Invoice_MerchantID, // << 使用發票專用 MerchantID >>
                RqHeader = new { Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }, // 時間戳 // 原有備註
                Data = encryptedData
            };
            string jsonRequestPayloadOuterString = JsonSerializer.Serialize(requestPayloadOuter, jsonOptions);
            _logger.LogInformation($"[Invoice] 完整請求 JSON (發送前): {jsonRequestPayloadOuterString}");

            var client = _httpClientFactory.CreateClient("ECPayInvoiceClient"); // 建議為 HttpClient 命名以区分 // 原有備註
            HttpContent httpContent = new StringContent(jsonRequestPayloadOuterString, Encoding.UTF8, "application/json");
            HttpResponseMessage response;
            try
            {
                // **重要**：確認此 Invoice_ApiUrl 在設定中正確 // 原有備註 (部分)
                response = await client.PostAsync(_ecpayConfig.Invoice_ApiUrl, httpContent); // **重要**：使用設定檔中的 Invoice_ApiUrl // 原有備註 (部分)
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[Invoice] 呼叫綠界開立發票 API 時發生 HttpRequestException。"); // 原有備註 + 更新
                return new Dictionary<string, string> { { "RtnCode", "HTTP_REQUEST_ERROR" }, { "RtnMsg", ex.Message } };
            }

            string responseString = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"[Invoice] 綠界開立發票 API 回應原始字串: {responseString}"); // 原有備註 + 更新

            // --- Step 7-10: 解析回應, AES解密, URL解碼, 解析內部JSON ---
            var apiResult = new Dictionary<string, string>();
            try
            {
                var responseOuter = JsonSerializer.Deserialize<ECPayInvoiceResponseOuter>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (responseOuter == null) throw new JsonException("無法解析外層回應JSON。");

                apiResult["TransCode"] = responseOuter.TransCode?.ToString();
                apiResult["TransMsg"] = responseOuter.TransMsg;

                if (responseOuter.TransCode == 1 && !string.IsNullOrEmpty(responseOuter.Data))
                {
                    string decryptedData = AESDecrypt(responseOuter.Data, _ecpayConfig.Invoice_HashKey, _ecpayConfig.Invoice_HashIV); // << 使用發票專用 Key/IV >>
                    string urlDecodedInnerData = HttpUtility.UrlDecode(decryptedData);
                    _logger.LogInformation($"[Invoice] 解密解碼後的回應 Data (JSON): {urlDecodedInnerData}");

                    var innerResponse = JsonSerializer.Deserialize<ECPayInvoiceResponseInner>(urlDecodedInnerData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (innerResponse == null) throw new JsonException("無法解析內層回應JSON。");

                    apiResult["RtnCode"] = innerResponse.RtnCode?.ToString();
                    apiResult["RtnMsg"] = innerResponse.RtnMsg;
                    apiResult["InvoiceNo"] = innerResponse.InvoiceNo;
                    apiResult["InvoiceDate"] = innerResponse.InvoiceDate;
                    apiResult["RandomNumber"] = innerResponse.RandomNumber;
                }
                else
                {
                    // 即使 TransCode 非1，或 Data 為空，也嘗試從外層取 RtnCode (如果存在，某些錯誤可能直接在外層顯示)
                    // 但主要還是依賴 TransCode 和解密後的 RtnCode
                    if (responseOuter.TransCode != 1)
                    {
                        _logger.LogWarning($"[Invoice] TransCode非1: {responseOuter.TransCode}, TransMsg: {responseOuter.TransMsg}. 原始回應: {responseString}");
                        apiResult["RtnCode"] = responseOuter.TransCode?.ToString() ?? "TRANS_CODE_ERROR"; // 將 TransCode 視為 RtnCode
                        apiResult["RtnMsg"] = responseOuter.TransMsg ?? "ECPay system level transaction failed.";
                    }
                    else
                    { // TransCode = 1 但 Data 為空
                        _logger.LogWarning($"[Invoice] TransCode為1但加密Data為空。原始回應: {responseString}");
                        apiResult["RtnCode"] = "MISSING_ENCRYPTED_DATA_IN_RESPONSE";
                        apiResult["RtnMsg"] = "ECPay system transaction OK but no encrypted data returned.";
                    }
                }
            }
            catch (Exception ex) // 捕捉 JsonException 或 AES 解密錯誤等
            {
                _logger.LogError(ex, $"[Invoice] 解析回應或解密時發生錯誤。原始回應: {responseString}");
                apiResult["RtnCode"] = "RESPONSE_PROCESSING_ERROR";
                apiResult["RtnMsg"] = ex.Message;
            }

            _logger.LogInformation("[Invoice] 解析後的回應：RtnCode={RtnCode}, RtnMsg={RtnMsg}, InvoiceNo={InvoiceNo}",
                apiResult.GetValueOrDefault("RtnCode"), apiResult.GetValueOrDefault("RtnMsg"), apiResult.GetValueOrDefault("InvoiceNo"));
            return apiResult;
        }

        // 測試用的臨時方法
        public void TestEncryption()
        {
            string key = "ejCk326UnaZWKisg";
            string iv = "q9jcZX8Ib9LM8wYk";

            // 測試案例 1: URL 編碼為大寫十六進位
            string plainTextUrlEncodedUpper = "%7B%22Name%22%3A%22Test%22%2C%22ID%22%3A%22A123456789%22%7D";
            string expectedEncryptedUpper = "uvI4yrErM37XNQkXGAgRgJAgHn2t72jahaMZzYhWL1HmvH4WV18VJDP2i9pTbC+tby5nxVExLLFyAkbjbS2Dvg==";
            string actualEncryptedUpper = AESEncrypt(plainTextUrlEncodedUpper, key, iv);
            _logger.LogInformation($"Test Case 1 (Upper): Expected: {expectedEncryptedUpper}, Actual: {actualEncryptedUpper}, Match: {expectedEncryptedUpper == actualEncryptedUpper}");

            // 測試案例 2: URL 編碼為小寫十六進位
            string plainTextUrlEncodedLower = "%7b%22name%22%3a%22test%22%2c%22id%22%3a%22a123456789%22%7d"; // 綠界範例中內部 JSON 的鍵也是小寫
                                                                                                             // 如果原始 JSON 是 {"Name":"Test","ID":"A123456789"}
                                                                                                             // 那麼小寫 URL Encode 應為 %7b%22Name%22%3a%22Test%22%2c%22ID%22%3a%22A123456789%22%7d
                                                                                                             // 而不是 %7b%22name%22%3a%22test%22...
                                                                                                             // 這裡我們先用綠界提供的小寫 URL 編碼結果來測試
            string plainTextUrlEncodedLowerFromExample = "%7b%22Name%22%3a%22Test%22%2c%22ID%22%3a%22A123456789%22%7d"; // 與綠界範例一致
            string expectedEncryptedLower = "ZD/z07UvdmL3aYz0tsVo+bFXF5VldNcns6ezyfea777KOmLiizrUNDYe+v1bh2QTT4AySf1NICgXxWXB6f7c6A==";
            string actualEncryptedLower = AESEncrypt(plainTextUrlEncodedLowerFromExample, key, iv);
            _logger.LogInformation($"Test Case 2 (Lower): Expected: {expectedEncryptedLower}, Actual: {actualEncryptedLower}, Match: {expectedEncryptedLower == actualEncryptedLower}");

            // 您的實際 urlEncodedData (來自日誌，它是小寫的)
            // string yourActualUrlEncodedData = "%7b%22MerchantID%22%3a%222000132%22%2c%22RelateNumber%22%3a%22TRV2505241845302604E%22%2c%22CustomerIdentifier%22%3a%22%22%2c%22CustomerName%22%3a%22%e8%91%89%e6%9b%84%e7%87%81%22%2c%22Print%22%3a%220%22%2c%22CustomerPhone%22%3a%22%2b8860905088127%22%2c%22CustomerEmail%22%3a%22123%403123.com%22%2c%22Donation%22%3a%220%22%2c%22LoveCode%22%3a%22%22%2c%22CarrierType%22%3a%22%22%2c%22CarrierNum%22%3a%22%22%2c%22TaxType%22%3a%221%22%2c%22vat%22%3a%221%22%2c%22Items%22%3a%5b%7b%22ItemSeq%22%3a1%2c%22ItemName%22%3a%22%e5%9c%8b%e5%a4%96%e6%97%85%e8%a1%8c%e5%b0%88%e6%a1%88%e6%a8%99%e9%a1%8c+-+%e6%88%90%e4%ba%ba%22%2c%22ItemCount%22%3a1%2c%22ItemWord%22%3a%22%e5%bc%8f%22%2c%22ItemPrice%22%3a10000.00%2c%22ItemAmount%22%3a10000.00%7d%5d%2c%22SalesAmount%22%3a10000%2c%22InvoiceRemark%22%3a%22%e8%a8%82%e5%96%ae%e7%b7%a8%e8%99%9f%3a+TRV2505241845302604E%22%2c%22InvType%22%3a%2207%22%7d";
            // string yourActualEncrypted = AESEncrypt(yourActualUrlEncodedData, _ecpayConfig.Invoice_HashKey, _ecpayConfig.Invoice_HashIV);
            // _logger.LogInformation($"Your actual encrypted data: {yourActualEncrypted}"); // 這應該跟你日誌中的 Bw+uBg... 相同
        }

        // --- AES 加密輔助方法 ---
        private string AESEncrypt(string plainText, string key, string iv)
        {
            try
            {
                using (Aes aesAlg = Aes.Create()) // 或者 new AesCryptoServiceProvider()
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(key);
                    aesAlg.IV = Encoding.UTF8.GetBytes(iv);
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                            csEncrypt.Write(plainTextBytes, 0, plainTextBytes.Length);
                            // << 確保 FlushFinalBlock 被呼叫 >>
                            csEncrypt.FlushFinalBlock();
                        }
                        byte[] encryptedBytes = msEncrypt.ToArray();
                        return Convert.ToBase64String(encryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AESEncrypt_Test] Encryption failed.");
                throw;
            }
        }

        // --- AES 解密輔助方法 ---
        private string AESDecrypt(string cipherTextBase64, string key, string iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                byte[] cipherTextBytes = Convert.FromBase64String(cipherTextBase64);
                using (MemoryStream msDecrypt = new MemoryStream(cipherTextBytes))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }

        // 輔助方法：清理發票品項名稱中的特殊字元
        private string SanitizeInvoiceItemName(string itemName)
        {
            if (string.IsNullOrEmpty(itemName)) return "商品"; // 提供預設值以防空品名 // 原有備註
            // 根據綠界文件，品項名稱不可包含「|」、「&」、「=」等特殊符號，這裡做簡單替換 // 原有備註
            return itemName.Replace("|", " ").Replace("&", " ").Replace("=", " ");
        }

        // GetParametersForVerification - 此方法是您原有的，用於從 CallbackViewModel 提取參數。
        // 如果 ProcessEcPayCallback 方法直接使用 IFormCollection，此方法可能不再直接被 ProcessEcPayCallback 呼叫。
        // 但保留它可能有其他用途，或作為參考。 (金流用 SHA256 CheckMacValue)
        // private SortedDictionary<string, string> GetParametersForVerification(ECPayCallbackViewModel callbackData, ILogger logger) { ... } // 原有備註


        /// <summary>
        /// 處理綠界支付回呼通知 (ReturnURL) - 綠界會非同步 POST 通知此端點
        /// </summary>
        /// <param name="formData">綠界 POST 回來的表單資料</param>
        /// <returns>回傳給綠界的回應字串 "1|OK" 或 "0|錯誤訊息"</returns>
        public async Task<string> ProcessEcPayCallback(IFormCollection formData)
        {
            _logger.LogInformation("[PaymentReturn] 收到綠界付款回呼。"); // 原有備註
            var paramsForVerification = new SortedDictionary<string, string>(StringComparer.Ordinal);
            string receivedCheckMacValue = formData["CheckMacValue"].FirstOrDefault(); // formData returns StringValues

            // 直接從 formData (即 Request.Form) 建立參數字典
            foreach (var key in formData.Keys)
            {
                if (!string.Equals(key, "CheckMacValue", StringComparison.OrdinalIgnoreCase))
                {
                    paramsForVerification.Add(key, formData[key].FirstOrDefault()); // 取第一個值
                }
            }

            // 記錄下實際從 Form 中提取並排序後的參數，用於比對
            _logger.LogInformation($"[PaymentReturn] Parameters DIRECTLY FROM FORM for local CheckMacValue calculation (sorted): {string.Join("&", paramsForVerification.Select(p => $"{p.Key}={p.Value}"))}"); // 原有備註

            if (string.IsNullOrEmpty(receivedCheckMacValue))
            {
                _logger.LogError($"[PaymentReturn] MerchantTradeNo: {formData["MerchantTradeNo"].FirstOrDefault()} 缺少 CheckMacValue。"); // 原有備註 + 更新
                return "0|Missing CheckMacValue"; // 或者根據您的錯誤處理策略 // 原有備註
            }

            // << 使用金流的 HashKey 和 HashIV 進行驗證 >>
            var calculatedCheckMacValue = GetCheckMacValue(paramsForVerification, _ecpayConfig.HashKey, _ecpayConfig.HashIV);
            string merchantTradeNoFromForm = formData["MerchantTradeNo"].FirstOrDefault() ?? "UNKNOWN_MTN"; // 原有備註

            if (!receivedCheckMacValue.Equals(calculatedCheckMacValue, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError($"[PaymentReturn] CheckMacValue 驗證失敗！特店交易編號: {merchantTradeNoFromForm}。收到: {receivedCheckMacValue}, 計算: {calculatedCheckMacValue}"); // 原有備註
                return "0|CheckMacValue Error"; // 原有備註
            }
            _logger.LogInformation($"[PaymentReturn] CheckMacValue 驗證成功 for MerchantTradeNo: {merchantTradeNoFromForm}"); // 原有備註

            // --- 訂單處理邏輯 ---
            var order = await _dbContext.Orders
                                        .Include(o => o.OrderDetails) // 開票需要 // 原有備註
                                        .Include(o => o.OrderInvoices) // 檢查是否已開票 // 原有備註
                                        .FirstOrDefaultAsync(o => o.MerchantTradeNo == merchantTradeNoFromForm);

            if (order == null)
            {
                _logger.LogError($"[PaymentReturn] 找不到對應的訂單，MerchantTradeNo: {merchantTradeNoFromForm}。"); // 原有備註
                return "1|OK_OrderNotFound"; // 即使找不到訂單，也可能需要回傳 "1|OK" 給綠界，避免它一直重試。// 原有備註 (部分)
            }

            int rtnCodeFromForm = int.TryParse(formData["RtnCode"].FirstOrDefault(), out int rc) ? rc : -1; // 提供預設值以防轉換失敗或欄位不存在 // 原有備註 (部分)
            string ecpayTradeNoFromForm = formData["TradeNo"].FirstOrDefault(); // 綠界交易編號，用於開票 // 原有備註 (部分)

            // 使用您更新後的 OrderStatus
            if (rtnCodeFromForm == 1) // 金流付款成功
            {
                // 避免重複處理已完成且已開票的訂單
                if (order.Status == OrderStatus.Completed && order.OrderInvoices.Any(inv => inv.InvoiceStatus == InvoiceStatus.Opened))
                {
                    _logger.LogInformation($"[PaymentReturn] 訂單 {order.OrderId} (MTN: {merchantTradeNoFromForm}) 已是完成狀態 ({order.Status}) 且已有成功發票，判斷為重複通知。"); // 原有備註 + 更新
                    return "1|OK_DuplicateNotification_CompletedAndInvoiced";
                }

                // 只有 Awaiting 狀態的訂單才繼續處理付款和開票
                if (order.Status == OrderStatus.Awaiting)
                {
                    _logger.LogInformation($"[PaymentReturn] 訂單 {order.OrderId} (MTN: {merchantTradeNoFromForm}) 付款成功。準備開立發票。"); // 原有備註
                    order.PaymentDate = DateTime.TryParse(formData["PaymentDate"].FirstOrDefault(), out var pDate) ? pDate : DateTime.UtcNow; // 原有備註
                    order.ECPayTradeNo = ecpayTradeNoFromForm; // 儲存綠界交易編號 // 原有備註

                    // << 修改點：預設訂單狀態為 Completed，如果發票失敗再改為 InvoiceFailed >>
                    order.Status = OrderStatus.Completed;

                    if (!order.OrderInvoices.Any(inv => inv.InvoiceStatus == InvoiceStatus.Opened)) // 如果還沒有成功開立的發票 // 原有備註
                    {
                        _logger.LogInformation($"[PaymentReturn] 訂單 {order.OrderId} 尚未開立發票，嘗試開立。"); // 原有備註
                        try
                        {
                            var invoiceResult = await IssueInvoiceAsync(order, ecpayTradeNoFromForm); // 呼叫開票方法 // 原有備註
                            string invoiceRtnCode = invoiceResult.GetValueOrDefault("RtnCode");
                            string invoiceNumber = invoiceResult.GetValueOrDefault("InvoiceNo");

                            var newInvoice = new OrderInvoice // 無論成功失敗都建立一筆記錄
                            {
                                OrderId = order.OrderId,
                                BuyerName = (order.InvoiceOption == InvoiceOption.Company && !string.IsNullOrWhiteSpace(order.InvoiceTitle)) ? order.InvoiceTitle : order.OrdererName, // 原有備註 (部分)
                                InvoiceItemDesc = string.Join(" | ", order.OrderDetails.Select(od => SanitizeInvoiceItemName(od.Description ?? "商品")).Take(3)) + (order.OrderDetails.Count > 3 ? "..." : ""), // 原有備註 + 更新品項分隔符
                                TotalAmount = order.TotalAmount, // 這裡通常存訂單金額 // 原有備註
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                InvoiceType = order.InvoiceOption == InvoiceOption.Company ? InvoiceType.Triplet : InvoiceType.ElectronicInvoice, // 原有備註 (部分)
                                BuyerUniformNumber = order.InvoiceOption == InvoiceOption.Company ? order.InvoiceUniformNumber : null, // 原有備註 (部分)
                                // << 修改點：儲存 RandomCode >>
                                RandomCode = invoiceResult.GetValueOrDefault("RandomNumber"),
                                Note = $"ECPay API RtnMsg: {invoiceResult.GetValueOrDefault("RtnMsg")}" // 將綠界API的訊息存到Note
                                // InvoiceFileURL: 綠界新版API文件未直接提供查詢PDF的URL，可能需透過廠商後台或另外的查詢API // 原有備註 (部分)
                            };

                            if (invoiceRtnCode == "1" && !string.IsNullOrEmpty(invoiceNumber))
                            {
                                _logger.LogInformation($"[PaymentReturn] 訂單 {order.OrderId} 的發票 {invoiceNumber} 已成功開立。"); // 原有備註
                                newInvoice.InvoiceNumber = invoiceNumber;
                                newInvoice.InvoiceStatus = InvoiceStatus.Opened; // 已開立發票 // 原有備註
                                // order.Status = OrderStatus.Completed; // 已在前面預設 // 原有備註 (部分)
                            }
                            else
                            {
                                _logger.LogError($"[PaymentReturn] 訂單 {order.OrderId} 開立發票失敗。RtnCode: {invoiceRtnCode}, RtnMsg: {invoiceResult.GetValueOrDefault("RtnMsg")}"); // 原有備註 + 更新
                                order.Status = OrderStatus.InvoiceFailed; // 付款成功但發票開立失敗 (您新增的狀態)
                                newInvoice.InvoiceStatus = InvoiceStatus.Pending; // 或新增 InvoiceStatus.Failed，Pending表示待處理此失敗
                                newInvoice.Note = $"開票失敗: RtnCode={invoiceRtnCode}, RtnMsg={invoiceResult.GetValueOrDefault("RtnMsg")}"; // 更新 Note
                            }
                            _dbContext.OrderInvoices.Add(newInvoice);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"[PaymentReturn] 訂單 {order.OrderId} 開立發票過程中發生例外錯誤。"); // 原有備註 + 更新
                            order.Status = OrderStatus.InvoiceFailed; // 付款成功但發票開立失敗
                            _dbContext.OrderInvoices.Add(new OrderInvoice
                            { // 記錄例外導致的失敗
                                OrderId = order.OrderId,
                                InvoiceStatus = InvoiceStatus.Pending, // 或 Failed
                                Note = $"開票例外: {ex.Message}", // 原有備註
                                TotalAmount = order.TotalAmount,
                                InvoiceType = order.InvoiceOption == InvoiceOption.Company ? InvoiceType.Triplet : InvoiceType.ElectronicInvoice,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                BuyerName = (order.InvoiceOption == InvoiceOption.Company && !string.IsNullOrWhiteSpace(order.InvoiceTitle)) ? order.InvoiceTitle : order.OrdererName,
                                BuyerUniformNumber = order.InvoiceOption == InvoiceOption.Company ? order.InvoiceUniformNumber : null,
                            });
                        }
                    }
                    else
                    { // 已存在成功開立的發票
                        _logger.LogInformation($"[PaymentReturn] 訂單 {order.OrderId} 已存在成功開立的發票，跳過重複開票。狀態直接設為 Completed。"); // 原有備註
                        if (order.Status != OrderStatus.Completed) order.Status = OrderStatus.Completed; // 確保是完成狀態
                    }
                    await _dbContext.SaveChangesAsync();
                    return "1|OK"; // 原有備註
                }
                else // 付款成功，但訂單狀態不符 (例如已完成但可能之前開票失敗，或重複通知)
                {
                    _logger.LogInformation($"[PaymentReturn] 訂單 {order.OrderId} (MTN: {merchantTradeNoFromForm}) 付款成功，但訂單狀態為 {order.Status}，可能已處理。"); // 原有備註 + 更新
                    // 考慮是否需要檢查 order.Status == OrderStatus.InvoiceFailed 並嘗試重新開票 (如果適用)
                    // if (order.Status == OrderStatus.InvoiceFailed && !order.OrderInvoices.Any(inv => inv.InvoiceStatus == InvoiceStatus.Opened)) {
                    //    _logger.LogInformation($"[PaymentReturn] 訂單 {order.OrderId} 先前發票開立失敗，考慮是否重試。");
                    //    // ... (類似上面的開票邏輯，但可能需要更謹慎的重複檢查和業務判斷) ...
                    // }
                    return "1|OK_PaymentSuccess_StateAlreadyProcessed"; // 原有備註名修改
                }
            }
            else // 金流付款失敗
            {
                // << 修改點：使用您更新的 OrderStatus >>
                if (order.Status == OrderStatus.Awaiting)
                {
                    order.Status = OrderStatus.Cancelled; // 付款失敗，訂單取消 (您更新的狀態)
                    order.ECPayTradeNo = ecpayTradeNoFromForm; // 仍然可以記錄綠界交易號碼 // 原有備註
                    await _dbContext.SaveChangesAsync();
                    _logger.LogWarning($"[PaymentReturn] 訂單 {order.OrderId} (MTN: {merchantTradeNoFromForm}) 支付失敗 (RtnCode: {rtnCodeFromForm}, RtnMsg: {formData["RtnMsg"].FirstOrDefault()})。狀態更新為 Cancelled。"); // 原有備註 + 更新
                }
                else
                {
                    _logger.LogWarning($"[PaymentReturn] 訂單 {order.OrderId} (MTN: {merchantTradeNoFromForm}) 支付失敗，但訂單狀態為 {order.Status}，可能已處理。"); // 原有備註 + 更新
                }
                return "1|OK_PaymentFailed"; // 即使付款失敗，通常也要回 "1|OK" 給綠界 // 原有備註 (部分)
            }
        }

        /// <summary>
        /// 處理綠界支付完成後，使用者瀏覽器被導向的頁面 (OrderResultURL)
        /// </summary>
        /// <param name="formData">綠界 POST 回來的參數</param>
        /// <param name="orderId">從 URL 路由中取得的訂單 ID (或從 CustomField1)</param>
        /// <returns>重導向到前端訂單結果頁面的 URL 字串</returns>
        public async Task<string> GetFrontendRedirectUrlAfterPayment(IFormCollection formData, int orderId)
        {
            _logger.LogInformation($"[PaymentResult] 處理 OrderResultURL for OrderId from route: {orderId}"); // 原有備註 + 更新
            var paramsForVerification = new SortedDictionary<string, string>(StringComparer.Ordinal);
            string receivedCheckMacValue = formData["CheckMacValue"].FirstOrDefault();
            string merchantTradeNoFromForm = formData["MerchantTradeNo"].FirstOrDefault() ?? $"UNKNOWN_MTN_FOR_ORDER_{orderId}"; // 原有備註

            foreach (var key in formData.Keys)
            {
                if (!string.Equals(key, "CheckMacValue", StringComparison.OrdinalIgnoreCase))
                {
                    paramsForVerification.Add(key, formData[key].FirstOrDefault());
                }
            }
            _logger.LogInformation($"[PaymentResult] Parameters DIRECTLY FROM FORM for OrderResultURL CheckMacValue calculation (sorted): {string.Join("&", paramsForVerification.Select(p => $"{p.Key}={p.Value}"))}"); // 原有備註

            if (string.IsNullOrEmpty(receivedCheckMacValue))
            {
                _logger.LogError($"[PaymentResult] OrderResultURL for MTN: {merchantTradeNoFromForm} (OrderId from route: {orderId}) 缺少 CheckMacValue。將導向失敗頁面。"); // 原有備註 + 更新
                // << 修改點：確保 orderIdForPathOnError 的來源正確 >>
                string orderIdForPathOnError = formData["CustomField1"].FirstOrDefault() ?? orderId.ToString(); // CustomField1 通常是您自己設定的訂單ID
                string failurePath = $"{_ecpayConfig.FrontendBaseUrl.TrimEnd('/')}/order-complete/{orderIdForPathOnError}"; // 使用統一的結果頁面路徑
                var errorParams = new Dictionary<string, string> {
                    { "status", "error" }, // 使用簡化的狀態
                    { "message", "missing_signature_client" }, // 原有備註 (部分)
                    { "mtn", merchantTradeNoFromForm }
                };
                return QueryHelpers.AddQueryString(failurePath, errorParams.Where(kvp => !string.IsNullOrEmpty(kvp.Value)));
            }

            var calculatedCheckMacValue = GetCheckMacValue(paramsForVerification, _ecpayConfig.HashKey, _ecpayConfig.HashIV); // 金流用 Key/IV
            if (!string.Equals(receivedCheckMacValue, calculatedCheckMacValue, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError($"[PaymentResult] OrderResultURL CheckMacValue 驗證失敗！MTN: {merchantTradeNoFromForm} (OrderId from route: {orderId})。收到: {receivedCheckMacValue}, 計算: {calculatedCheckMacValue}."); // 原有備註 + 更新
                string orderIdForPathOnError = formData["CustomField1"].FirstOrDefault() ?? orderId.ToString();
                string failurePath = $"{_ecpayConfig.FrontendBaseUrl.TrimEnd('/')}/order-complete/{orderIdForPathOnError}";
                var errorParams = new Dictionary<string, string> {
                     { "status", "error" }, // 使用簡化的狀態
                     { "message", "signature_error_client" }, // 原有備註 (部分)
                     { "mtn", merchantTradeNoFromForm }
                };
                return QueryHelpers.AddQueryString(failurePath, errorParams.Where(kvp => !string.IsNullOrEmpty(kvp.Value)));
            }
            _logger.LogInformation($"[PaymentResult] OrderResultURL CheckMacValue 驗證成功 for MTN: {merchantTradeNoFromForm} (OrderId from route: {orderId})"); // 原有備註 + 更新


            // 從 CustomField1 (通常是您自己設定的訂單ID) 或路由參數 orderId 獲取最終用於前端跳轉的 orderId
            string finalOrderIdForRedirect = formData["CustomField1"].FirstOrDefault() ?? orderId.ToString();

            // 準備給前端重新導向 URL 的查詢參數
            var frontendQueryParams = new Dictionary<string, string>
            {
                ["rtnCode"] = formData["RtnCode"].FirstOrDefault(),
                ["merchantTradeNo"] = merchantTradeNoFromForm,
                ["tradeNo"] = formData["TradeNo"].FirstOrDefault(),
                ["paymentDate"] = formData["PaymentDate"].FirstOrDefault()?.Replace(" ", "T"), // 'T' for JS Date parsing
                ["paymentType"] = formData["PaymentType"].FirstOrDefault(),
                ["tradeAmt"] = formData["TradeAmt"].FirstOrDefault(), // 交易金額
                ["customField2"] = formData["CustomField2"].FirstOrDefault(), // 您自訂的欄位
                ["customField3"] = formData["CustomField3"].FirstOrDefault(), // 您自訂的欄位
            };

            int rtnCodeFromForm = int.TryParse(frontendQueryParams["rtnCode"], out int rc) ? rc : -1;

            string resultPagePath = "/order-complete/"; // 您的前端訂單結果頁面相對路徑，例如 /checkout/result/ 或 /order-complete/
            string baseUrl = $"{_ecpayConfig.FrontendBaseUrl.TrimEnd('/')}{resultPagePath}{finalOrderIdForRedirect}";


            if (rtnCodeFromForm == 1)
            { // 付款成功
                frontendQueryParams["status"] = "success";
                // RtnMsg 通常是 "交易成功" 或類似，前端可以直接顯示綠界回傳的，或者您自訂更友好的訊息
                frontendQueryParams["rtnMsg"] = formData["RtnMsg"].FirstOrDefault() ?? "付款成功"; // 原有備註 (部分)
            }
            else
            { // 付款失敗或其他非成功狀態
                frontendQueryParams["status"] = "failure";
                frontendQueryParams["rtnMsg"] = formData["RtnMsg"].FirstOrDefault() ?? "付款失敗"; // 原有備註 (部分)
            }

            string finalFrontendUrl = QueryHelpers.AddQueryString(baseUrl, frontendQueryParams.Where(kvp => !string.IsNullOrEmpty(kvp.Value)));
            _logger.LogInformation($"[PaymentResult] 將返回的前端 URL: {finalFrontendUrl}"); // 原有備註
            return finalFrontendUrl;
        }

        /// <summary>
        /// 計算 CheckMacValue (綠界金流驗證碼 - SHA256)
        /// </summary>
        /// <param name="orderParams">已按鍵名排序的參數字典 (使用 StringComparer.Ordinal)</param>
        /// <param name="hashKey">綠界提供的 HashKey (金流用)</param>
        /// <param name="hashIV">綠界提供的 HashIV (金流用)</param>
        /// <returns>計算出的 CheckMacValue 字串</returns>
        private string GetCheckMacValue(SortedDictionary<string, string> orderParams, string hashKey, string hashIV)
        {
            // 步驟1: 將參數依 Key 名稱字母排序 (SortedDictionary 已處理) // 原有備註
            // 步驟2: 將參數依照 "Key=Value" 格式用 "&"串接 // 原有備註
            var joinedParameters = string.Join("&", orderParams.Select(pair => $"{pair.Key}={pair.Value}"));
            // _logger.LogDebug($"GetCheckMacValue - 1. 已排序並串接的參數: {joinedParameters}"); // 原有備註

            // 步驟3: 頭尾加上 HashKey 和 HashIV // 原有備註
            var stringToEncode = $"HashKey={hashKey}&{joinedParameters}&HashIV={hashIV}";
            // _logger.LogDebug($"GetCheckMacValue - 2. 加上 HashKey 和 HashIV (準備URL編碼): {stringToEncode}"); // 原有備註

            // 步驟4: 對整個字串進行 URL 編碼 // 原有備註
            string urlEncodedString = HttpUtility.UrlEncode(stringToEncode);
            // _logger.LogDebug($"GetCheckMacValue - 3. URL 編碼後: {urlEncodedString}"); // 原有備註

            // 步驟5: 將 URL 編碼後的字串轉為小寫 // 原有備註
            string lowerCaseString = urlEncodedString.ToLower();
            // _logger.LogDebug($"GetCheckMacValue - 4. 轉為小寫後: {lowerCaseString}"); // 原有備註

            // _logger.LogDebug($"GetCheckMacValue - String before SHA256 Hashing (urlEncoded, lowercased, with HashKey/IV): {lowerCaseString}"); // 原有備註
            // 步驟6: 使用 SHA256 進行加密 // 原有備註
            using (SHA256 algorithm = SHA256.Create())
            {
                byte[] hashBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(lowerCaseString));
                // 步驟7: 將 SHA256 加密結果轉為大寫 // 原有備註
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToUpper();
            }
        }
    }
}