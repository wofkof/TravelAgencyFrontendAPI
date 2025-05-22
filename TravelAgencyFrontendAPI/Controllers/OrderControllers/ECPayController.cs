// ECPayController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models; 
using ECPay.Models; // ECPay SDK 的 Models (IPayment, Payment, PaymentResult)
using ECPay.Services; // ECPay SDK 的 Services (CheckMac)
using ECPay.Enumeration; // ECPay SDK 的 Enums
using System.Text; // For StringBuilder
using System.Web; // For HttpUtility (如果目標框架支援，或者用 System.Net.WebUtility.UrlEncode)
using Microsoft.Extensions.Configuration; // For IConfiguration
using Microsoft.Extensions.Logging;
using ECPay.Services.Checkout; // For ILogger

namespace TravelAgencyFrontendAPI.Controllers // 您的 Controller 命名空間
{
    [Route("api/[controller]")]
    [ApiController]
    public class ECPayController : ControllerBase // 注意：通常 API Controller 繼承 ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ECPayController> _logger;
        private readonly string _merchantID;
        private readonly string _hashKey;
        private readonly string _hashIV;
        private readonly string _ecpayApiUrl; // 綠界支付 API URL
        private readonly string _ecpayReturnUrl; // 您的後端URL
        private readonly string _ecpayClientBackUrlBase; // 前端支付結果頁面URL

        // 通過 DI 注入依賴
        public ECPayController(AppDbContext context, ILogger<ECPayController> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;

            _merchantID = configuration["ECPaySettings:MerchantID"];
            _hashKey = configuration["ECPaySettings:HashKey"];
            _hashIV = configuration["ECPaySettings:HashIV"];
            _ecpayApiUrl = configuration["ECPaySettings:ApiUrl"]; 
            _ecpayReturnUrl = configuration["ECPaySettings:ReturnUrl"];
            _ecpayClientBackUrlBase = configuration["ECPaySettings:ClientBackUrlBase"];

            // 檢查配置是否已載入
            if (string.IsNullOrEmpty(_merchantID) || string.IsNullOrEmpty(_hashKey) || string.IsNullOrEmpty(_hashIV) || string.IsNullOrEmpty(_ecpayApiUrl) || string.IsNullOrEmpty(_ecpayReturnUrl))
            {
                _logger.LogError("ECPay 設定未完整載入，請檢查 appsettings.json。");
                // 也可以考慮拋出一個啟動錯誤
            }
        }

        [HttpPost("Checkout/{orderId}")] // 例如：POST /api/ECPay/Checkout/123
        public async Task<IActionResult> PrepareECPayPayment(int orderId)
        {
            // 驗證用戶身份 (假設您有類似 GetCurrentUserId 的方法)
            var currentUserId = GetCurrentUserId(); 
            if (currentUserId == null)
            {
                _logger.LogWarning("PrepareECPayPayment - 未授權的使用者嘗試支付訂單 {OrderId}", orderId);
                return Unauthorized("使用者未登入。");
            }

            // 1. 從資料庫取得訂單資料，並包含必要的關聯資料
            var order = await _context.Orders
                .Include(o => o.OrderDetails) // 只載入 OrderDetails 集合
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.MemberId == currentUserId.Value);

            if (order == null)
            {
                _logger.LogWarning("PrepareECPayPayment - 找不到訂單 {OrderId} 或使用者 {UserId} 無權限。", orderId, currentUserId);
                return NotFound($"找不到訂單 (ID: {orderId}) 或您無權限進行此操作。");
            }

            if (order.Status != OrderStatus.Awaiting)
            {
                _logger.LogWarning("PrepareECPayPayment - 訂單 {OrderId} 狀態為 {Status}，非待付款狀態。", orderId, order.Status);
                return BadRequest($"訂單 (ID: {orderId}) 目前狀態為 {order.Status}，無法進行付款。");
            }

            if (order.PaymentMethod.HasValue)
            {
                switch (order.PaymentMethod.Value)
                {
                    case PaymentMethod.ECPay_CreditCard:
                        // 準備 ECPay 信用卡支付
                        break;
                    case PaymentMethod.LinePay:
                        _logger.LogInformation($"訂單 {order.OrderId} 選擇 LINE Pay，由此 ECPayController 處理是錯誤的。");
                        return BadRequest("選擇的支付方式為 LINE Pay，請透過正確的路徑處理。");

                    default:
                        _logger.LogError($"訂單 {order.OrderId} 請求了無法識別的支付方式代碼: {order.PaymentMethod.Value}。");
                        return BadRequest("系統無法識別所選的付款方式。");
                }
            }
            else
            {
                _logger.LogError($"訂單 {order.OrderId} 未指定支付方式。");
                return BadRequest("未指定付款方式。");
            }

            // 2. 手動載入 OrderDetail 關聯的 GroupTravel 和 CustomTravel (優化 N+1)
            if (order.OrderDetails != null && order.OrderDetails.Any())
            {
                var groupTravelIds = order.OrderDetails
                                        .Where(od => od.Category == ProductCategory.GroupTravel)
                                        .Select(od => od.ItemId)
                                        .Distinct()
                                        .ToList();
                var customTravelIds = order.OrderDetails
                                        .Where(od => od.Category == ProductCategory.CustomTravel)
                                        .Select(od => od.ItemId)
                                        .Distinct()
                                        .ToList();

                List<GroupTravel> groupTravels = new List<GroupTravel>();
                if (groupTravelIds.Any())
                {
                    // 確保載入產生 ItemName 所需的關聯資料
                    groupTravels = await _context.GroupTravels
                                            .Where(gt => groupTravelIds.Contains(gt.GroupTravelId))
                                            .Include(gt => gt.OfficialTravelDetail)
                                            .ThenInclude(otd => otd.OfficialTravel) // 為了 OfficialTravel.Title
                                            .ToListAsync();
                }

                List<CustomTravel> customTravels = new List<CustomTravel>();
                if (customTravelIds.Any())
                {
                    customTravels = await _context.CustomTravels
                                             .Where(ct => customTravelIds.Contains(ct.CustomTravelId))
                                             .ToListAsync(); // CustomTravel 的 Note 通常直接在 CustomTravel 物件上
                }

                foreach (var detail in order.OrderDetails)
                {
                    if (detail.Category == ProductCategory.GroupTravel)
                    {
                        detail.GroupTravel = groupTravels.FirstOrDefault(gt => gt.GroupTravelId == detail.ItemId);
                    }
                    else if (detail.Category == ProductCategory.CustomTravel)
                    {
                        detail.CustomTravel = customTravels.FirstOrDefault(ct => ct.CustomTravelId == detail.ItemId);
                    }
                }
            }

            // 3. 創建 ECPay PaymentConfiguration 並填充資料
            var paymentConfig = new ECPay.Services.Checkout.PaymentConfiguration();

            string merchantTradeNo = $"ORD{order.OrderId}{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            // TradeDesc 需要 UrlEncode
            string tradeDesc = System.Net.WebUtility.UrlEncode($"訂單 {order.OrderId} - {order.OrdererName}");
            DateTime merchantTradeDate = order.CreatedAt; // 或 DateTime.Now，根據 ECPay 要求
            int totalAmount = (int)order.TotalAmount;

            // 準備 ItemName (ECPay 的商品名稱)
            var itemNamesForEcpay = new List<string>();
            if (order.OrderDetails != null && order.OrderDetails.Any())
            {
                foreach (var detail in order.OrderDetails)
                {
                    string productName = "商品"; // 預設名稱
                    if (detail.Category == ProductCategory.GroupTravel && detail.GroupTravel?.OfficialTravelDetail?.OfficialTravel != null)
                    {
                        productName = detail.GroupTravel.OfficialTravelDetail.OfficialTravel.Title;
                    }
                    else if (detail.Category == ProductCategory.CustomTravel && detail.CustomTravel != null)
                    {
                        productName = !string.IsNullOrEmpty(detail.CustomTravel.Note) ? detail.CustomTravel.Note : $"客製化行程-{detail.ItemId}";
                    }
                    else if (!string.IsNullOrEmpty(detail.Description)) // 作為備用
                    {
                        productName = detail.Description;
                    }
                    // 您可以根據需求調整顯示的格式，例如是否包含價格和數量
                    itemNamesForEcpay.Add($"{productName}");
                }
            }
            else
            {
                itemNamesForEcpay.Add($"訂單 {order.OrderId} 總額"); // 如果沒有明細的備用名稱
            }
            // ItemName 參數也需要 UrlEncode，且多個品項用 # 分隔
            string finalItemName = System.Net.WebUtility.UrlEncode(string.Join("#", itemNamesForEcpay));

            var itemDetailsForSdk = order.OrderDetails.Select(d =>
            {
                string productName = "商品"; // Default name
                if (d.Category == ProductCategory.GroupTravel && d.GroupTravel?.OfficialTravelDetail?.OfficialTravel != null)
                    productName = d.GroupTravel.OfficialTravelDetail.OfficialTravel.Title;
                else if (d.Category == ProductCategory.CustomTravel && d.CustomTravel != null)
                    productName = !string.IsNullOrEmpty(d.CustomTravel.Note) ? d.CustomTravel.Note : $"客製化行程-{d.ItemId}";
                else if (!string.IsNullOrEmpty(d.Description))
                    productName = d.Description;

                return new TestCheckoutItem // SDK uses TestCheckoutItem
                {
                    Name = productName, // Name should be the name of a single item
                    Price = (int)d.Price, // Price per unit for this item detail
                    Quantity = d.Quantity
                };
            }).ToList();

            try
            {
                // 需要注意每個方法返回的是 IPaymentConfiguration
                paymentConfig.Send.ToApi(_ecpayApiUrl);
                paymentConfig.Send.ToMerchant(_merchantID, storeId: null, isPlatform: false); // 明確傳遞可選參數
                paymentConfig.Send.UsingHash(_hashKey, _hashIV);

                // 設定 Return 部分 (Return 屬性的方法也是返回 IPaymentConfiguration)
                paymentConfig.Return.ToServer(_ecpayReturnUrl);
                paymentConfig.Return.ToClient($"{_ecpayClientBackUrlBase}{order.OrderId}");

                // 設定 Transaction 部分 (Transaction 屬性的方法也是返回 IPaymentConfiguration)
                paymentConfig.Transaction.New(merchantTradeNo, tradeDesc, merchantTradeDate);
                paymentConfig.Transaction.UseMethod(EPaymentMethod.Credit);
                paymentConfig.Transaction.WithItems(itemDetailsForSdk, amount: totalAmount);

                // 最後產生請求
                IPayment finalPaymentRequest = paymentConfig.Generate();

                _logger.LogInformation("ECPay 請求參數 (經SDK PaymentConfiguration產生): {@ECPayRequest}", finalPaymentRequest);

                // 5. 產生自動提交的 HTML 表單
                string htmlForm = GenerateAutoPostForm(finalPaymentRequest.URL, ToDictionary(finalPaymentRequest));
                return Content(htmlForm, "text/html; charset=utf-8");
            }
            catch (ArgumentNullException argEx)
            {
                _logger.LogError(argEx, "準備 ECPay 付款請求時發生參數錯誤。訂單 ID: {OrderId}. 錯誤參數: {ParamName}, 訊息: {ErrorMessage}", orderId, argEx.ParamName, argEx.Message);
                return BadRequest($"準備付款資訊時發生參數錯誤: {argEx.ParamName} 為必填但未設定。({argEx.Message})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "準備 ECPay 付款請求時發生未預期錯誤。訂單 ID: {OrderId}", orderId);
                return StatusCode(StatusCodes.Status500InternalServerError, "準備付款資訊時發生錯誤，請稍後再試。");
            }
        }

        // 您的 ECPayReturn Callback Action (已存在，但需注入 ILogger 和 IConfiguration)
        [HttpPost("Callback")]
        public async Task<IActionResult> ECPayReturn([FromForm] ECPay.Models.PaymentResult result)
        {
            _logger.LogInformation("ECPay Callback Received: {@Result}", result);

            // 將 result 物件轉換成 Dictionary<string, string> 以便日誌或除錯
            var callbackParams = new Dictionary<string, string>();
            // ... (將 result 轉換為 callbackParams 的程式碼如前所述) ...
            foreach (var prop in result.GetType().GetProperties())
            {
                var value = prop.GetValue(result);
                if (value != null) { callbackParams.Add(prop.Name, value.ToString()); }
            }
            _logger.LogInformation("ECPay Callback Form Data: {@FormData}", callbackParams);


            // 驗證 CheckMacValue (從設定檔讀取 HashKey/IV)
            bool isValid = ECPay.Services.CheckMac.PaymentResultIsValid(result, _hashKey, _hashIV); // 使用 DI 的 _hashKey, _hashIV

            if (!isValid)
            {
                _logger.LogError("ECPay Callback CheckMacValue 驗證失敗。訂單號：{MerchantTradeNo}", result.MerchantTradeNo);
                return Content("0|FAIL");
            }

            // 嘗試從 MerchantTradeNo 解析出 OrderId（例如格式為 ORD1234-abc12345）
            var orderIdPart = result.MerchantTradeNo?.Split('-').FirstOrDefault()?.Replace("ORD", "");
            if (!int.TryParse(orderIdPart, out int orderId))
            {
                _logger.LogError("ECPay Callback 無法從 MerchantTradeNo 解析出 OrderId: {MerchantTradeNo}", result.MerchantTradeNo);
                return Content("0|FAIL");
            }

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                _logger.LogError("ECPay Callback 找不到訂單 OrderId={OrderId}", orderId);
                return Content("0|FAIL");
            }

            if (result.RtnCode == 1) // 付款成功
            {
                order.Status = OrderStatus.Pending;
                order.PaymentDate = DateTime.UtcNow;
                order.ECPayTradeNo = result.TradeNo; // 可以在 Order 模型中新增這個欄位
                order.MerchantTradeNo = result.MerchantTradeNo;

                await _context.SaveChangesAsync();
                _logger.LogInformation("訂單 {OrderId} 付款成功，狀態已更新。", orderId);
                return Content("1|OK"); // 必須回傳這個格式告訴綠界 Callback 接收成功
            }
            else
            {
                _logger.LogWarning("付款失敗或取消，訂單號：{MerchantTradeNo}，錯誤訊息：{RtnMsg}", result.MerchantTradeNo, result.RtnMsg);
                return Content("1|OK"); // 即便失敗，也要回傳 1|OK，否則綠界會持續重送 Callback
            }
        }

        // 輔助方法：獲取當前登入使用者的 MemberId (您需要根據您的身份驗證機制來實作)
        private int? GetCurrentUserId()
        {
            // 範例：如果您使用 JWT，可以從 Claims 中解析
            // var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // if (int.TryParse(userIdClaim, out int userId)) return userId;
            // return null;
            return 11110; // 暫時返回一個測試 ID，您需要替換它
        }

        // 輔助方法：將 IPayment 物件轉換成字典，以便產生 POST 表單
        private Dictionary<string, string> ToDictionary(IPayment payment)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in typeof(IPayment).GetProperties())
            {
                var value = prop.GetValue(payment);
                if (value != null)
                {
                    dict.Add(prop.Name, value.ToString());
                }
            }
            return dict;
        }

        // 輔助方法：產生自動 POST 的 HTML 表單
        private string GenerateAutoPostForm(string actionUrl, Dictionary<string, string> parameters)
        {
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'><title>Redirecting to ECPay...</title></head><body onload='document.forms[0].submit()'>");
            sb.AppendFormat("<form method='post' action='{0}'>", actionUrl);
            _logger.LogInformation("--- ECPay Form Parameters (to be submitted to ECPay) ---");
            foreach (var item in parameters.OrderBy(x => x.Key)) // 建議排序以方便查看日誌
            {
                // CheckMacValue 不應被 HtmlEncode，其他參數理論上在傳入前就應做好 UrlEncode (例如 TradeDesc, ItemName)
                // 但為了安全，對 value 再次 HtmlEncode 以避免 XSS (綠界端會解碼)
                // 不過，由於綠界API的參數很多是特定格式，過度編碼可能導致問題，需小心。
                // 通常 SDK 處理過的參數，直接放入 hidden input 即可。
                // TradeDesc 和 ItemName 在賦值給 payment 物件時已經 UrlEncode 過了。
                sb.AppendFormat("<input type='hidden' name='{0}' value='{1}' />", item.Key, HttpUtility.HtmlAttributeEncode(item.Value));
                _logger.LogInformation("Param: {Key} = {Value}", item.Key, item.Value);
            }
            sb.Append("<noscript><p>正在重導向到綠界支付頁面，如果瀏覽器沒有自動跳轉，請點擊下面的按鈕。</p><input type='submit' value='前往付款' /></noscript>");
            sb.Append("</form></body></html>");
            return sb.ToString();
        }
    }
}