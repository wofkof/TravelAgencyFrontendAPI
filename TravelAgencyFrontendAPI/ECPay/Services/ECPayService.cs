// 路徑：TravelAgencyFrontendAPI/ECPay/Services/ECPayService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography; // 用於 SHA256
using System.Text;
using System.Threading.Tasks;
using System.Web; // 用於 HttpUtility.UrlEncode，可能需要安裝 NuGet 套件 System.Web.HttpUtility
using Microsoft.Extensions.Options; // 用於 IOptions
using TravelAgency.Shared.Data; // 假設您的 DbContext 在這裡
using TravelAgency.Shared.Models; // 假設您的 Order 模型在這裡
using TravelAgencyFrontendAPI.ECPay.Models; // 您的新 ECPay 模型
using Microsoft.EntityFrameworkCore; // 用於 FindAsync 和 SaveChangesAsync
using Microsoft.Extensions.Configuration; // 用於讀取 HostURL
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc; // 用於 FromFormAttribute (雖然通常在 Controller 使用，但輔助方法可能參考其 Name)
using System.Reflection; // 用於日誌記錄 和 反射屬性
using Microsoft.AspNetCore.WebUtilities;


namespace TravelAgencyFrontendAPI.ECPay.Services
{
    public class ECPayService
    {
        private readonly ECPayConfiguration _ecpayConfig;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ECPayService> _logger;
        private readonly string _hostUrl; // 您的後端 API 部署網址

        public class ECPayPaymentRequestViewModel
        {
            public string EcPayAioCheckOutUrl { get; set; } // 綠界結帳頁面網址
            public Dictionary<string, string> Parameters { get; set; } // 送往綠界的參數
        }

        public ECPayService(IOptions<ECPayConfiguration> ecpayConfig, AppDbContext dbContext, IConfiguration configuration, ILogger<ECPayService> logger)
        {
            _ecpayConfig = ecpayConfig.Value;
            _dbContext = dbContext;
            _logger = logger;
            _hostUrl = configuration.GetValue<string>("HostURL") ?? throw new ArgumentNullException("HostURL is not configured in appSettings.json");
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
                _logger.LogWarning($"OrderId: {orderId} 的訂單未找到。");
                throw new ArgumentException($"找不到訂單 ID 為 {orderId} 的訂單。");
            }

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Awaiting)
            {
                _logger.LogWarning($"OrderId: {orderId} 的訂單狀態不是待處理或等待中。當前狀態: {order.Status}");
                throw new InvalidOperationException($"訂單 ID 為 {orderId} 的訂單狀態不允許支付。");
            }

            // --- 特店交易編號 (MerchantTradeNo) 生成邏輯 ---
            // 確保 MerchantTradeNo長度不超過20碼，且只包含英數字
            var dateTimeString = DateTime.Now.ToString("yyyyMMddHHmmssfff"); // 時間戳，包含毫秒以增加唯一性
            var orderIdPart = order.OrderId.ToString();
            string prefix = "TRV";

            // 預計時間戳部分長度，例如 "yyyyMMddHHmmss" 為 14碼
            // 您可以根據實際需要調整此處，確保最終 MerchantTradeNo 不超過20碼
            string timePart = DateTime.Now.ToString("yyyyMMddHHmmss"); // 使用到秒的時間戳

            string merchantTradeNoBase = $"{prefix}{orderIdPart}{timePart}";
            string merchantTradeNo = new string(merchantTradeNoBase.Where(char.IsLetterOrDigit).ToArray());

            if (merchantTradeNo.Length > 20)
            {
                // 如果組合後超過20碼，需要進行截斷
                // 一種策略是優先保留 Prefix 和 OrderIdPart，然後截斷 timePart
                int availableLengthForTime = 20 - prefix.Length - orderIdPart.Length;
                if (availableLengthForTime > 0)
                {
                    timePart = timePart.Substring(0, Math.Min(timePart.Length, availableLengthForTime));
                    merchantTradeNo = $"{prefix}{orderIdPart}{timePart}";
                }
                else // 如果 Prefix + OrderIdPart 就已經很長
                {
                    merchantTradeNo = $"{prefix}{orderIdPart}".Substring(0, 20);
                }
                merchantTradeNo = new string(merchantTradeNo.Where(char.IsLetterOrDigit).ToArray()); // 再次確保只有英數字
            }
            _logger.LogInformation($"最終 MerchantTradeNo: {merchantTradeNo} (長度: {merchantTradeNo.Length})");


            order.MerchantTradeNo = merchantTradeNo;
            await _dbContext.SaveChangesAsync(); // 確保 MerchantTradeNo 已儲存

            // --- 商品名稱 (ItemName) 和 交易描述 (TradeDesc) ---
            var itemName = string.Join("#", order.OrderDetails.Select(od => $"{od.Description} x {od.Quantity}"));
            if (string.IsNullOrEmpty(itemName)) itemName = "旅遊行程商品";
            if (itemName.Length > 200) itemName = itemName.Substring(0, 197) + "...";

            var tradeDesc = $"旅遊訂單號: {order.OrderId}";
            if (!string.IsNullOrEmpty(order.Note) && (tradeDesc.Length + order.Note.Length + 3) < 200)
            {
                tradeDesc += $" ({order.Note})";
            }
            if (tradeDesc.Length > 200) tradeDesc = tradeDesc.Substring(0, 197) + "...";

            // --- 準備送往綠界的參數 ---
            // 使用 SortedDictionary 並指定 StringComparer.Ordinal 以確保參數名按字母順序排序（區分大小寫）
            var orderParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                { "MerchantID", _ecpayConfig.MerchantID },
                { "MerchantTradeNo", merchantTradeNo },
                //{ "RtnCode", "1" }, // 這是綠界要求的參數，表示交易狀態
                //{ "RtnMsg", "OK" }, // 這是綠界要求的參數，表示回傳訊息
                //{ "TradeNo", merchantTradeNo }, // 這是綠界要求的參數，表示交易編號
                //{ "TradeDate", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") }, // 交易日期
                //{ "TradeAmt", (order.TotalAmount).ToString() }, // 總金額，轉換為整數
                { "TotalAmount", ((int)order.TotalAmount).ToString() }, // 總金額，轉換為整數
                { "CustomField1", order.OrderId.ToString()}, // 訂單 ID
                { "CustomField2", order.OrdererEmail ?? "" }, // 訂單者 Email
                { "CustomField3", order.OrdererPhone ?? "" }, // 訂單者電話
                { "CustomField4", "" }, // 自訂欄位4
                { "MerchantTradeDate", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") },
                { "PaymentType", "aio" },
                { "ChoosePayment", "Credit" },
                { "TradeDesc", tradeDesc },
                { "ItemName", itemName },
                { "ReturnURL", $"{_hostUrl.TrimEnd('/')}/api/ECPay/Callback" }, // 伺服器端背景通知URL
                { "OrderResultURL", $"{_hostUrl.TrimEnd('/')}/api/ECPay/Return/{order.OrderId}" }, // 使用者端前景通知URL
                { "EncryptType", "1" }, // 使用 SHA256 加密

                // 可選參數:
                // { "ClientBackURL", $"{_hostUrl.TrimEnd('/')}/your-frontend-order-page/{order.OrderId}" }, // 付款頁面左上角的「返回商店」按鈕連結
            };

            //if (!string.IsNullOrEmpty(order.OrdererEmail))
            //{
            //    orderParams.Add("CustomField1", order.OrderId.ToString()); // 自訂欄位範例
            //    orderParams.Add("CustomField2", order.OrdererEmail); // 訂單者 Email
            //    orderParams.Add("CustomField3", order.OrdererPhone); // 訂單者電話
            //}

            var checkMacValue = GetCheckMacValue(orderParams, _ecpayConfig.HashKey, _ecpayConfig.HashIV);
            orderParams.Add("CheckMacValue", checkMacValue);

            _logger.LogInformation($"已為訂單 ID: {orderId}, 綠界特店交易編號: {merchantTradeNo} 生成 ECPay 付款參數物件。");

            return new ECPayPaymentRequestViewModel
            {
                EcPayAioCheckOutUrl = _ecpayConfig.ECPayAioCheckOutUrl, // 綠界金流介接的目標 URL
                Parameters = orderParams.ToDictionary(pair => pair.Key, pair => pair.Value) // 轉換為一般字典給前端
            };
        }

        /// <summary>
        /// 從 ECPayCallbackViewModel 提取參數以供 CheckMacValue 驗證。
        /// </summary>
        /// <param name="callbackData">從綠界收到的回呼資料模型。</param>
        /// <param name="logger">日誌記錄器。</param>
        /// <returns>一個已排序的字典，包含用於驗證的參數。</returns>
        private SortedDictionary<string, string> GetParametersForVerification(ECPayCallbackViewModel callbackData, ILogger logger)
        {
            // 使用 StringComparer.Ordinal 確保參數名按字母順序排序（區分大小寫）
            var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal);
            PropertyInfo[] properties = callbackData.GetType().GetProperties();

            foreach (PropertyInfo prop in properties)
            {
                string paramName = prop.Name; // 預設使用屬性名稱

                // 檢查是否有 [FromForm(Name="...")] 或 [JsonPropertyName("...")] 屬性
                // 以便獲取綠界實際使用的參數名稱（如果C#屬性名稱與綠界參數名稱的大小寫或拼寫不同）
                var fromFormAttr = prop.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromFormAttribute>();
                var jsonPropertyNameAttr = prop.GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>();

                if (fromFormAttr != null && !string.IsNullOrEmpty(fromFormAttr.Name))
                {
                    paramName = fromFormAttr.Name;
                }
                else if (jsonPropertyNameAttr != null && !string.IsNullOrEmpty(jsonPropertyNameAttr.Name))
                {
                    paramName = jsonPropertyNameAttr.Name;
                }

                // 跳過 CheckMacValue 欄位本身
                if (string.Equals(paramName, "CheckMacValue", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                object valueObj = prop.GetValue(callbackData);
                string paramValue = valueObj?.ToString() ?? ""; // 將 null 物件轉換為空字串

                // 確保使用綠界參數的原始名稱（區分大小寫）作為字典的鍵
                if (!parameters.ContainsKey(paramName))
                {
                    parameters.Add(paramName, paramValue);
                }
                else
                {
                    // 理論上不應發生，除非 ECPayCallbackViewModel 中有重複的參數對應
                    logger.LogWarning($"準備 CheckMacValue 參數時遇到重複的鍵: {paramName}。將使用屬性 {prop.Name} 的值。");
                    parameters[paramName] = paramValue;
                }
            }
            _logger.LogDebug($"GetParametersForVerification - 準備用於 CheckMacValue 計算的參數（排序後，加入 Key/IV 前）: {string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"))}");
            return parameters;
        }


        /// <summary>
        /// 處理綠界支付回呼通知 (ReturnURL) - 綠界會非同步 POST 通知此端點
        /// </summary>
        /// <param name="callbackData">綠界 POST 回來的參數，已自動綁定到模型</param>
        /// <returns>回傳給綠界的回應字串 "1|OK" 或 "0|錯誤訊息"</returns>
        public async Task<string> ProcessEcPayCallback(Microsoft.AspNetCore.Http.IFormCollection formData)
        {
            _logger.LogInformation($"收到綠界回呼通知 (ReturnURL).");

            var paramsForVerification = new SortedDictionary<string, string>(StringComparer.Ordinal);
            string receivedCheckMacValue = "";

            // 直接從 formData (即 Request.Form) 建立參數字典
            foreach (var key in formData.Keys)
            {
                if (string.Equals(key, "CheckMacValue", StringComparison.OrdinalIgnoreCase))
                {
                    receivedCheckMacValue = formData[key].ToString();
                }
                else
                {
                    // formData[key] 返回 StringValues，通常對於表單欄位是單一值，直接 ToString()
                    paramsForVerification.Add(key, formData[key].ToString());
                }
            }

            // 記錄下實際從 Form 中提取並排序後的參數，用於比對
            _logger.LogInformation($"Parameters DIRECTLY FROM FORM for local CheckMacValue calculation (sorted): {string.Join("&", paramsForVerification.Select(p => $"{p.Key}={p.Value}"))}");

            if (string.IsNullOrEmpty(receivedCheckMacValue))
            {
                _logger.LogError($"綠界回呼 (ReturnURL) MerchantTradeNo: {formData["MerchantTradeNo"]} 缺少 CheckMacValue。");
                return "0|Missing CheckMacValue"; // 或者根據您的錯誤處理策略
            }

            var calculatedCheckMacValue = GetCheckMacValue(paramsForVerification, _ecpayConfig.HashKey, _ecpayConfig.HashIV);

            // 從 formData 中獲取業務邏輯需要的欄位
            string merchantTradeNoFromForm = formData.TryGetValue("MerchantTradeNo", out var mtnValues) ? mtnValues.ToString() : "UNKNOWN_MTN";
            int rtnCodeFromForm = formData.TryGetValue("RtnCode", out var rcValues) && int.TryParse(rcValues.ToString(), out int rc) ? rc : -1; // 提供預設值以防轉換失敗或欄位不存在
            string rtnMsgFromForm = formData.TryGetValue("RtnMsg", out var rmValues) ? rmValues.ToString() : "No RtnMsg";


            if (!receivedCheckMacValue.Equals(calculatedCheckMacValue, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError($"綠界回呼 (ReturnURL) CheckMacValue 驗證失敗！特店交易編號: {merchantTradeNoFromForm}。收到: {receivedCheckMacValue}, 計算: {calculatedCheckMacValue}");
                return "0|CheckMacValue Error";
            }
            _logger.LogInformation($"綠界回呼 (ReturnURL) CheckMacValue 驗證成功 for MerchantTradeNo: {merchantTradeNoFromForm}");

            // --- 後續的訂單處理邏輯，請將之前使用 callbackData.XXX 的地方， ---
            // --- 修改為從 formData["XXX"].ToString() 或已解析的變數 (如 merchantTradeNoFromForm, rtnCodeFromForm) 獲取值 ---

            string parsedOrderIdString;
            try
            {
                const string prefix = "TRV";
                // 確保 merchantTradeNoFromForm 不是預設的 UNKNOWN_MTN
                if (merchantTradeNoFromForm != "UNKNOWN_MTN" && merchantTradeNoFromForm.StartsWith(prefix))
                {
                    // 調整時間戳長度或OrderId提取邏輯以符合您MerchantTradeNo的實際生成規則
                    int timeStampLength = 14; // 假設固定14位
                    if (merchantTradeNoFromForm.Length > prefix.Length + timeStampLength)
                    {
                        parsedOrderIdString = merchantTradeNoFromForm.Substring(prefix.Length, merchantTradeNoFromForm.Length - prefix.Length - timeStampLength);
                    }
                    else
                    {
                        parsedOrderIdString = new string(merchantTradeNoFromForm.Substring(prefix.Length).TakeWhile(char.IsDigit).ToArray());
                    }
                }
                else
                {
                    _logger.LogError($"無法從 MerchantTradeNo ({merchantTradeNoFromForm}) 中解析訂單 ID，前綴或格式不符。");
                    return "0|Invalid MerchantTradeNo for OrderId parsing";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"從 MerchantTradeNo ({merchantTradeNoFromForm}) 解析訂單 ID 時發生錯誤。");
                return "0|Error parsing OrderId from MerchantTradeNo";
            }

            if (!int.TryParse(parsedOrderIdString, out int orderId))
            {
                _logger.LogError($"從 MerchantTradeNo ({merchantTradeNoFromForm}) 解析出的訂單 ID ('{parsedOrderIdString}') 不是有效的整數。");
                return "0|Invalid parsed OrderId from MerchantTradeNo";
            }

            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
            {
                _logger.LogError($"找不到訂單 ID: {orderId} (來自 MTN: {merchantTradeNoFromForm})。");
                return "1|OK"; // 告知綠界已收到，但內部記錄錯誤
            }

            if (rtnCodeFromForm == 1)
            {
                if (order.Status == OrderStatus.Pending || order.Status == OrderStatus.Awaiting)
                {
                    order.Status = OrderStatus.Completed;
                    order.PaymentDate = formData.TryGetValue("PaymentDate", out var pdValues) && DateTime.TryParse(pdValues.ToString(), out var pDate) ? pDate : DateTime.Now;
                    order.ECPayTradeNo = formData.TryGetValue("TradeNo", out var tnValues) ? tnValues.ToString() : null;
                    // order.MerchantTradeNo 應該與 merchantTradeNoFromForm 一致
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation($"訂單 ID: {order.OrderId} 支付成功。狀態已更新為 'Completed'。");
                    return "1|OK";
                }
                else
                {
                    _logger.LogInformation($"訂單 ID: {order.OrderId} 已被處理或處於最終狀態 ({order.Status})。綠界回報成功 (RtnCode: 1)。");
                    return "1|OK";
                }
            }
            else
            {
                if (order.Status == OrderStatus.Pending || order.Status == OrderStatus.Awaiting)
                {
                    order.Status = OrderStatus.Cancelled;
                    order.ECPayTradeNo = formData.TryGetValue("TradeNo", out var tnValues) ? tnValues.ToString() : null;
                    await _dbContext.SaveChangesAsync();
                    _logger.LogWarning($"訂單 ID: {order.OrderId} 支付失敗/非成功狀態 (RtnCode: {rtnCodeFromForm}, RtnMsg: {rtnMsgFromForm})。狀態已更新為 'Cancelled'。");
                    return "1|OK";
                }
                else
                {
                    _logger.LogWarning($"訂單 ID: {order.OrderId} 已被處理或處於最終狀態 ({order.Status})，且收到失敗/非成功通知 (RtnCode: {rtnCodeFromForm})。");
                    return "1|OK";
                }
            }
        }

        /// <summary>
        /// 處理綠界支付完成後，使用者瀏覽器被導向的頁面 (OrderResultURL)
        /// 主要用於將使用者導回前端的訂單結果頁面，並可選擇性地再次驗證 CheckMacValue。
        /// </summary>
        /// <param name="callbackData">綠界 POST 回來的參數</param>
        /// <param name="orderId">從 URL 路由中取得的訂單 ID</param>
        /// <returns>重導向到前端訂單結果頁面的 URL 字串</returns>
        // 在 ECPayService.cs
        // 在 ECPayService.cs 檔案中
        public async Task<string> GetFrontendRedirectUrlAfterPayment(Microsoft.AspNetCore.Http.IFormCollection formData, int orderId)
        {
            _logger.LogInformation($"處理綠界 OrderResultURL。訂單 ID: {orderId}");

            var paramsForVerification = new SortedDictionary<string, string>(StringComparer.Ordinal);
            string receivedCheckMacValue = "";
            string merchantTradeNoFromForm = formData.TryGetValue("MerchantTradeNo", out var mtnValues) ? mtnValues.ToString() : $"UNKNOWN_MTN_FOR_ORDER_{orderId}";

            foreach (var key in formData.Keys)
            {
                if (string.Equals(key, "CheckMacValue", StringComparison.OrdinalIgnoreCase))
                {
                    receivedCheckMacValue = formData[key].ToString();
                }
                else
                {
                    paramsForVerification.Add(key, formData[key].ToString());
                }
            }
            _logger.LogInformation($"Parameters DIRECTLY FROM FORM for OrderResultURL CheckMacValue calculation (sorted): {string.Join("&", paramsForVerification.Select(p => $"{p.Key}={p.Value}"))}");

            if (string.IsNullOrEmpty(receivedCheckMacValue))
            {
                _logger.LogError($"綠界回傳 (OrderResultURL) for MerchantTradeNo: {merchantTradeNoFromForm} (訂單 ID: {orderId}) 缺少 CheckMacValue。將導向失敗頁面。");
                // 假設 _ecpayConfig.FrontendFailureUrl 或類似的設定指向一個通用的失敗/錯誤頁面路徑
                string failurePath = _ecpayConfig.FrontendFailureUrl ?? $"{_ecpayConfig.FrontendBaseUrl.TrimEnd('/')}/order-complete/{orderId}";
                var errorParams = new Dictionary<string, string> {
            { "status", "failed" },
            { "error", "missing_signature_client" },
            { "orderId", orderId.ToString()},
            { "MerchantTradeNo", merchantTradeNoFromForm } // 將商店交易號碼也傳過去，方便追蹤
        };
                return QueryHelpers.AddQueryString(failurePath, errorParams.Where(kvp => !string.IsNullOrEmpty(kvp.Value)));
            }

            var calculatedCheckMacValue = GetCheckMacValue(paramsForVerification, _ecpayConfig.HashKey, _ecpayConfig.HashIV);
            if (!string.Equals(receivedCheckMacValue, calculatedCheckMacValue, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError($"綠界回傳 (OrderResultURL) CheckMacValue 驗證失敗！MerchantTradeNo: {merchantTradeNoFromForm} (訂單 ID: {orderId})。收到: {receivedCheckMacValue}, 計算: {calculatedCheckMacValue}.");
                string failurePath = _ecpayConfig.FrontendFailureUrl ?? $"{_ecpayConfig.FrontendBaseUrl.TrimEnd('/')}/order-complete/{orderId}";
                var errorParams = new Dictionary<string, string> {
            { "status", "failed" },
            { "error", "signature_error_client" },
            { "orderId", orderId.ToString()},
            { "MerchantTradeNo", merchantTradeNoFromForm } // 將商店交易號碼也傳過去
        };
                return QueryHelpers.AddQueryString(failurePath, errorParams.Where(kvp => !string.IsNullOrEmpty(kvp.Value)));
            }
            else
            {
                _logger.LogInformation($"綠界回傳 (OrderResultURL) CheckMacValue 驗證成功 for MerchantTradeNo: {merchantTradeNoFromForm} (訂單 ID: {orderId})");
            }

            // 準備給前端重新導向 URL 的查詢參數
            var frontendQueryParams = new Dictionary<string, string>();

            // 從綠界的 formData 中提取所有相關資料
            int rtnCodeFromForm = formData.TryGetValue("RtnCode", out var rcVal) && int.TryParse(rcVal.ToString(), out int rc) ? rc : -1;
            string rtnMsgFromForm = formData.TryGetValue("RtnMsg", out var rmVal) ? rmVal.ToString() : "";

            frontendQueryParams["RtnCode"] = rtnCodeFromForm.ToString();
            frontendQueryParams["RtnMsg"] = rtnMsgFromForm;
            frontendQueryParams["MerchantTradeNo"] = merchantTradeNoFromForm; // 已提取的商店交易號碼
            frontendQueryParams["TradeNo"] = formData.TryGetValue("TradeNo", out var tnVal) ? tnVal.ToString() : ""; // 綠界的交易編號
            frontendQueryParams["TradeAmt"] = formData.TryGetValue("TradeAmt", out var taVal) ? taVal.ToString() : ""; // 交易金額
            frontendQueryParams["PaymentDate"] = formData.TryGetValue("PaymentDate", out var pdVal) ? pdVal.ToString() : ""; // 付款時間
            frontendQueryParams["PaymentType"] = formData.TryGetValue("PaymentType", out var ptVal) ? ptVal.ToString() : ""; // 付款方式
            frontendQueryParams["PaymentTypeChargeFee"] = formData.TryGetValue("PaymentTypeChargeFee", out var ptcfVal) ? ptcfVal.ToString() : ""; // 交易手續費

            // 自訂欄位 (請確保這些欄位在您最初的付款請求中有傳送)
            frontendQueryParams["CustomField1"] = formData.TryGetValue("CustomField1", out var cf1Val) ? cf1Val.ToString() : ""; // 通常是您的訂單ID
            frontendQueryParams["CustomField2"] = formData.TryGetValue("CustomField2", out var cf2Val) ? cf2Val.ToString() : ""; // 前端期望用此欄位顯示 Email
            frontendQueryParams["CustomField3"] = formData.TryGetValue("CustomField3", out var cf3Val) ? cf3Val.ToString() : ""; // 前端期望用此欄位顯示電話
            frontendQueryParams["CustomField4"] = formData.TryGetValue("CustomField4", out var cf4Val) ? cf4Val.ToString() : "";

            string frontendOrderCompletePath = "/order-complete"; // 您現有邏輯中使用的路徑
            string finalFrontendUrl;

            if (rtnCodeFromForm == 1) // 付款成功
            {
                frontendQueryParams["status"] = "success";
                frontendQueryParams["RtnMsg"] = "付款成功"; // 您希望顯示的文字
                // 作為額外檢查，再次確認資料庫中的訂單狀態
                var order = await _dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == orderId && o.MerchantTradeNo == merchantTradeNoFromForm);
                if (order != null && order.Status == OrderStatus.Completed)
                { // 或 Awaiting (如果是非即時付款方式)
                    _logger.LogInformation($"訂單 ID {orderId} 在 OrderResultURL 處理時，已於資料庫確認為完成狀態。");
                }
                else
                {
                    _logger.LogWarning($"訂單 ID {orderId} 綠界回傳 RtnCode=1，但資料庫狀態為 {order?.Status.ToString() ?? "找不到訂單"}。仍將以成功狀態導向至前端。");
                    frontendQueryParams["status"] = "pending_confirmation"; // 或根據您的流程調整
                }
            }
            else // 付款失敗或其他非成功狀態
            {
                frontendQueryParams["status"] = "failed";
            }

            // 建構最終要導向回您 Vue.js 前端的 URL
            string baseUrl = $"{_ecpayConfig.FrontendBaseUrl.TrimEnd('/')}{frontendOrderCompletePath}/{orderId}";
            // 在建立查詢字串前，過濾掉空的查詢參數值
            finalFrontendUrl = QueryHelpers.AddQueryString(baseUrl, frontendQueryParams.Where(kvp => !string.IsNullOrEmpty(kvp.Value)));

            _logger.LogInformation($"GetFrontendRedirectUrlAfterPayment 將返回的前端 URL: {finalFrontendUrl}");
            return finalFrontendUrl;
        }
        // --- 準備前端重導向 URL ---
        //string frontendSuccessUrl = _ecpayConfig.FrontendSuccessUrl;
        //string frontendFailureUrl = _ecpayConfig.FrontendFailureUrl;

        //    if (rtnCodeFromForm == 1)
        //    {
        //        var order = await _dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == orderId && o.MerchantTradeNo == merchantTradeNoFromForm);
        //        if (order != null)
        //        {
        //            if (order.Status == OrderStatus.Completed)
        //            {
        //                _logger.LogInformation($"訂單 ID: {orderId} (OrderResultURL) 支付成功 (DB確認)，重導向至前端成功頁面。");
        //                return $"{frontendSuccessUrl.TrimEnd('/')}?orderId={orderId}&status=success&tradeNo={HttpUtility.UrlEncode(merchantTradeNoFromForm)}";
        //            }
        //            // 判斷是否為非即時付款方式 (例如ATM, CVS)
        //            string paymentTypeFromForm = formData.TryGetValue("PaymentType", out var ptValues) ? ptValues.ToString() : "";
        //            if (order.Status == OrderStatus.Awaiting || (order.Status == OrderStatus.Pending && (paymentTypeFromForm?.StartsWith("ATM_") == true || paymentTypeFromForm?.StartsWith("CVS_") == true || paymentTypeFromForm?.StartsWith("BARCODE_") == true)))
        //            {
        //                _logger.LogInformation($"訂單 ID: {orderId} (OrderResultURL) 為非即時付款或等待確認 (DB狀態: {order.Status}, ECPay RtnCode: 1)，導向成功/處理中頁面。");
        //                return $"{frontendSuccessUrl.TrimEnd('/')}?orderId={orderId}&status=pending_confirmation&paymentType={HttpUtility.UrlEncode(paymentTypeFromForm)}&tradeNo={HttpUtility.UrlEncode(merchantTradeNoFromForm)}";
        //            }
        //            else
        //            {
        //                _logger.LogWarning($"訂單 ID: {orderId} (OrderResultURL) ECPay回傳RtnCode=1，但DB狀態為 {order.Status}。可能背景Callback延遲或狀態不一致。導向通用成功/查詢頁面。");
        //                return $"{frontendSuccessUrl.TrimEnd('/')}?orderId={orderId}&status=received_processing&tradeNo={HttpUtility.UrlEncode(merchantTradeNoFromForm)}";
        //            }
        //        }
        //        else
        //        {
        //            _logger.LogWarning($"訂單 ID: {orderId} (OrderResultURL) ECPay回傳RtnCode=1，但資料庫中找不到該訂單 (MTN: {merchantTradeNoFromForm})。導向通用成功/查詢頁面。");
        //            return $"{frontendSuccessUrl.TrimEnd('/')}?orderId={orderId}&status=received_unknown_order&tradeNo={HttpUtility.UrlEncode(merchantTradeNoFromForm)}";
        //        }
        //    }
        //    else
        //    {
        //        _logger.LogWarning($"訂單 ID: {orderId} (OrderResultURL) 操作失敗/取消 (RtnCode: {rtnCodeFromForm})，導向前端失敗頁面。");
        //        return $"{frontendFailureUrl.TrimEnd('/')}?orderId={orderId}&status=failed&rtnCode={rtnCodeFromForm}&rtnMsg={HttpUtility.UrlEncode(rtnMsgFromForm)}";
        //    }
        //}

        /// <summary>
        /// 計算 CheckMacValue (綠界金流驗證碼)
        /// </summary>
        /// <param name="orderParams">已按鍵名排序的參數字典 (使用 StringComparer.Ordinal)</param>
        /// <param name="hashKey">綠界提供的 HashKey</param>
        /// <param name="hashIV">綠界提供的 HashIV</param>
        /// <returns>計算出的 CheckMacValue 字串</returns>
        private string GetCheckMacValue(SortedDictionary<string, string> orderParams, string hashKey, string hashIV)
        {
            // 步驟1: 將參數依 Key 名稱字母排序 (SortedDictionary 已處理)
            // 步驟2: 將參數依照 "Key=Value" 格式用 "&"串接
            var joinedParameters = string.Join("&", orderParams.Select(pair => $"{pair.Key}={pair.Value}"));
            _logger.LogDebug($"GetCheckMacValue - 1. 已排序並串接的參數: {joinedParameters}");

            // 步驟3: 頭尾加上 HashKey 和 HashIV
            var stringToEncode = $"HashKey={hashKey}&{joinedParameters}&HashIV={hashIV}";
            _logger.LogDebug($"GetCheckMacValue - 2. 加上 HashKey 和 HashIV (準備URL編碼): {stringToEncode}");

            // 步驟4: 對整個字串進行 URL 編碼 (注意：.NET 的 HttpUtility.UrlEncode 與 PHP 的 urlencode 在空白字元處理上可能不同，
            // PHP 的 urlencode 會將空白轉成 '+'，而 .NET 的 HttpUtility.UrlEncode 預設轉成 '%20'。
            // 綠界官方 PHP 範例是用 urlencode。如果綠界期望的是'+'，則需額外處理。
            // 但根據綠界文件通常指示 "Parameter 請先做 UrlEncode"。且多數情況下，參數值本身不應包含需特殊編碼的原始空白。
            // 這裡我們遵循標準 .NET 的 UrlEncode。
            string urlEncodedString = HttpUtility.UrlEncode(stringToEncode);
            _logger.LogDebug($"GetCheckMacValue - 3. URL 編碼後: {urlEncodedString}");

            // 根據綠界文件，URL 編碼後的字串要轉為小寫。
            // 綠界文件範例中，若有 "+" (例如來自空格的編碼)，轉小寫後依然是 "+"。
            // .NET 的 HttpUtility.UrlEncode 預設將空格轉為 %20。%20 轉小寫仍為 %20。
            // 如果綠界嚴格要求 PHP urlencode 的行為（空格轉+），則需 string urlEncodedString = HttpUtility.UrlEncode(stringToEncode).Replace("%20", "+");
            // 但通常建議參數值本身先處理掉不必要的空格。
            // 目前保持標準 UrlEncode 後直接轉小寫。

            // 步驟5: 將 URL 編碼後的字串轉為小寫
            string lowerCaseString = urlEncodedString.ToLower();
            _logger.LogDebug($"GetCheckMacValue - 4. 轉為小寫後: {lowerCaseString}");

            _logger.LogDebug($"GetCheckMacValue - String before SHA256 Hashing (urlEncoded, lowercased, with HashKey/IV): {lowerCaseString}");
            // 步驟6: 使用 SHA256 進行加密
            string sha256Hash;
            using (SHA256 algorithm = SHA256.Create())
            {
                byte[] hashBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(lowerCaseString));
                sha256Hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
            }
            _logger.LogDebug($"GetCheckMacValue - 5. SHA256 加密後 (轉為十六進制前): {sha256Hash}");

            // 步驟7: 將 SHA256 加密結果轉為大寫
            string finalCheckMacValue = sha256Hash.ToUpper();
            _logger.LogDebug($"GetCheckMacValue - 6. 最終 CheckMacValue (大寫): {finalCheckMacValue}");

            return finalCheckMacValue;
        }
    }
}