// 路徑：TravelAgencyFrontendAPI/Controllers/OrderControllers/ECPayController.cs

using Microsoft.AspNetCore.Mvc;
using TravelAgencyFrontendAPI.ECPay.Services; // 引入您剛建立的 ECPayService
using TravelAgencyFrontendAPI.ECPay.Models; // 引入 ECPay 相關模型
using Microsoft.Extensions.Logging; // 引入日誌記錄
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options; // 用於 IFormCollection (雖然現在使用 FromForm 屬性會自動綁定) 和 Redirect

namespace TravelAgencyFrontendAPI.Controllers.OrderControllers
{
    [ApiController]
    [Route("api/[controller]")] // 定義 API 路由前綴為 /api/ECPay
    public class ECPayController : ControllerBase
    {
        private readonly ECPayService _ecpayService;
        private readonly ILogger<ECPayController> _logger;
        private readonly ECPayConfiguration _ecpayConfig;

        public ECPayController(ECPayService ecpayService, ILogger<ECPayController> logger, IOptions<ECPayConfiguration> ecpayConfigOptions)
        {
            _ecpayService = ecpayService;
            _logger = logger;
            _ecpayConfig = ecpayConfigOptions.Value;
        }

        /// <summary>
        /// [前端呼叫] 發起綠界支付請求。
        /// 前端 (OrderForm.vue) 會 POST 訂單 ID 到此 API，後端生成綠界支付所需的 HTML 表單並回傳。
        /// 前端收到後，會將瀏覽器重導向到綠界的支付頁面。
        /// 路由：POST /api/ECPay/Checkout/{orderId}
        /// </summary>
        /// <param name = "orderId" > 要支付的訂單 ID</param>
        /// <returns>包含綠界支付 HTML 表單的 JSON 物件</returns>
        [HttpPost("Checkout/{orderId}")]
        [Produces("application/json")]
        public async Task<IActionResult> Checkout(int orderId)
        {
            try
            {
                _logger.LogInformation($"收到訂單 ID: {orderId} 的結帳請求。");
                // 調用修改後的 Service 方法，該方法現在回傳 ECPayPaymentRequestViewModel
                var paymentParametersViewModel = await _ecpayService.GenerateEcPayPaymentForm(orderId); // 或者您改名後的 GenerateEcPayPaymentParameters

                // 直接回傳這個 ViewModel，ASP.NET Core 會將其序列化為 JSON
                // 前端就能收到 { "ecPayAioCheckOutUrl": "...", "parameters": { ... } }
                return Ok(paymentParametersViewModel);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"訂單 {orderId} 結帳錯誤: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, $"訂單 {orderId} 結帳錯誤: {ex.Message}");
                return StatusCode(409, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"處理訂單 {orderId} 結帳時發生未預期錯誤。");
                return StatusCode(500, new { message = "發生未預期錯誤，請稍後再試。" });
            }
        }
        /// <summary>
        /// [綠界呼叫] 綠界支付完成後，會非同步 POST 通知到此 URL (ReturnURL)。
        /// 這個端點用於後端接收綠界通知，並更新訂單狀態。
        /// 路由：POST /api/ECPay/Callback
        /// </summary>
        /// <param name="callbackData">綠界 POST 過來的表單資料會自動綁定到此模型</param>
        /// <returns>綠界要求的回覆字串 "OK"</returns>
        [HttpPost("Callback")]
        [Consumes("application/x-www-form-urlencoded")] // 指定消費的內容類型為表單數據
        [Produces("text/plain")] // 回傳純文字 "OK" 或其他訊息
        public async Task<IActionResult> Callback(IFormCollection form)
        {
            // 您已有的日誌記錄 Raw POST Form Data 可以保留或移至 Service
            var rawFormParameters = new List<string>();
            foreach (var key in form.Keys) { rawFormParameters.Add($"{key}={form[key]}"); }
            _logger.LogInformation($"ECPay Callback - Raw POST Form Data from IFormCollection: {string.Join("&", rawFormParameters)}");

            // 將整個 form 傳遞給 Service
            string serviceResponse = await _ecpayService.ProcessEcPayCallback(form);

            if (serviceResponse == "1|OK")
            {
                return Content("1|OK", "text/plain");
            }
            else
            {
                // 即使內部處理失敗，通常也建議回傳 "1|OK" 給綠界避免重試，錯誤已由內部日誌記錄
                _logger.LogError($"ECPay Callback 處理出現問題，但仍返回 '1|OK' 給 ECPay。內部服務回應: '{serviceResponse}'");
                return Content("1|OK", "text/plain");
            }
        }

        /// <summary>
        /// [綠界呼叫] 綠界支付完成後，會將使用者瀏覽器導向此 URL (ClientRedirectURL / OrderResultURL)。
        /// 這個端點主要用於將使用者重導向回前端的訂單結果頁面。
        /// 路由：POST /api/ECPay/Return/{orderId}
        /// </summary>
        /// <param name="orderId">從路由中取得的訂單 ID</param>
        /// <param name="callbackData">綠界 POST 過來的表單資料會自動綁定到此模型</param>
        /// <returns>重導向到前端的訂單結果頁面</returns>
        [HttpPost("Return/{orderId}")] // <--- 請確保這裡有 [HttpPost] 屬性
        [Consumes("application/x-www-form-urlencoded")] // 指定消費的內容類型為表單數據
        public async Task<IActionResult> Return(int orderId, IFormCollection form) // 改為接收 IFormCollection
        {
            _logger.LogInformation($"收到綠界 ClientReturn (OrderResultURL)。訂單 ID: {orderId}");
            _logger.LogInformation($"ECPay ClientReturn (OrderResultURL) - Raw POST Form Data from IFormCollection: {string.Join("&", form.Select(kv => $"{kv.Key}={kv.Value}"))}");
            try
            {
                // 記錄一下 OrderResultURL 收到的 Raw POST Data
                var rawFormParameters = new List<string>();
                foreach (var key in form.Keys) { rawFormParameters.Add($"{key}={form[key]}"); }
                _logger.LogInformation($"ECPay ClientReturn (OrderResultURL) - Raw POST Form Data from IFormCollection: {string.Join("&", rawFormParameters)}");

                // 將整個 form 傳遞給 Service
                string redirectUrl = await _ecpayService.GetFrontendRedirectUrlAfterPayment(form, orderId);
                _logger.LogInformation($"GetFrontendRedirectUrlAfterPayment 返回的重導向 URL: {redirectUrl}");

                if (string.IsNullOrEmpty(redirectUrl))
                {
                    _logger.LogError($"GetFrontendRedirectUrlAfterPayment 為訂單 {orderId} 返回了空或無效的重導向 URL。導向預設失敗頁。");
                    return Redirect(_ecpayConfig.FrontendFailureUrl + $"?orderId={orderId}&error=internal_redirect_error");
                }
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"處理訂單 {orderId} 的 ClientReturn (OrderResultURL) 時發生錯誤。");
                return Redirect(_ecpayConfig.FrontendFailureUrl + $"?orderId={orderId}&error=exception");
            }
        }
        // 您也可以新增一個用於查詢訂單狀態的 API，但這通常需要綠界提供專門的查詢 API
        // [HttpGet("Query/{orderId}")]
        // public async Task<IActionResult> QueryPaymentStatus(int orderId)
        // {
        //    // 實作查詢綠界交易狀態的邏輯，使用 ECPayService 中的 GetQueryCallBack 或類似方法
        //    // 這需要綠界提供查詢 API，並正確處理參數和簽章
        //    return Ok();
        // }
    }
}