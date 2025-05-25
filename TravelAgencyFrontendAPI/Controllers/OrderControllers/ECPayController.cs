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
        [HttpPost("ECPayReturn")]
        [Consumes("application/x-www-form-urlencoded")] // 指定消費的內容類型為表單數據
        [Produces("text/plain")] // 回傳純文字 "OK" 或其他訊息
        public async Task<IActionResult> ECPayReturn(IFormCollection form)
        {
            _logger.LogInformation("ECPayReturn (背景通知) 已被呼叫。");

            var rawFormParameters = new List<string>();
            foreach (var key in form.Keys) { rawFormParameters.Add($"{key}={form[key]}"); }
            _logger.LogInformation($"ECPayReturn - Raw POST Form Data from IFormCollection: {string.Join("&", rawFormParameters)}");

            try
            {
                // 將整個 form 傳遞給 Service 進行處理 (包含驗證 CheckMacValue、更新訂單、開立發票)
                string serviceResponse = await _ecpayService.ProcessEcPayCallback(form);

                // ProcessEcPayCallback 應該會回傳一個指示處理結果的字串，
                // 例如 "1|OK" (綠界要求的成功回應), "0|ErrorMessage" (內部錯誤，但仍可能需回1|OK給綠界)
                // 或者更細緻的內部狀態碼。

                _logger.LogInformation($"ECPayService.ProcessEcPayCallback 處理完成，回應: '{serviceResponse}'");

                // 根據綠界文件，通常無論內部處理細節如何 (除非是 CheckMacValue 這種嚴重錯誤)，
                // 只要收到通知，都應該回傳 "1|OK" 給綠界，避免它重試。
                // 內部的錯誤應透過日誌或其他方式記錄和處理。
                if (serviceResponse.StartsWith("1|")) // 假設 Service 處理成功或雖有內部問題但仍決定告知綠界OK
                {
                    return Content(serviceResponse, "text/plain"); // 直接回傳 Service 的回應，例如 "1|OK" 或 "1|OK_InvoiceFailed"
                }
                else if (serviceResponse.StartsWith("0|")) // 假設 Service 判斷為嚴重錯誤，需告知綠界失敗
                {
                    _logger.LogError($"ECPayReturn 偵測到 Service 處理失敗: '{serviceResponse}'。將回傳此錯誤訊息給 ECPay。");
                    return Content(serviceResponse, "text/plain"); // 例如 "0|CheckMacValue Error"
                }
                else // 未預期回應格式
                {
                    _logger.LogError($"ECPayService.ProcessEcPayCallback 返回未預期的回應格式: '{serviceResponse}'。預設回傳 1|OK 給綠界。");
                    return Content("1|OK_UnknownServiceResponse", "text/plain");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理 ECPayReturn 時發生未預期的 Controller 層例外錯誤。");
                // 發生嚴重例外，可能無法安全回覆綠界。但通常仍建議回覆1|OK避免重試，錯誤已記錄。
                return Content("1|OK_ControllerException", "text/plain");
            }
        }

        /// <summary>
        /// [綠界呼叫] 綠界支付完成後，會將使用者瀏覽器導向此 URL (OrderResultURL)。
        /// 這個端點主要用於將使用者重導向回前端的訂單結果頁面。
        /// 路由：POST /api/ECPay/ECPayOrderResult (確保與 ECPayService 中設定的 OrderResultURL 路徑一致)
        /// </summary>
        [HttpPost("ECPayOrderResult")] // **修改點：確保路由名稱與 Service 中設定的 OrderResultURL 一致**
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> ECPayOrderResult(IFormCollection form) // **修改點：方法名稱建議與路由對應，orderId 通常由 form 或 CustomField 傳遞**
        {
            // START: 修改 Return 方法 (或重命名為 ECPayOrderResult)
            _logger.LogInformation("ECPayOrderResult (客戶端前景導向) 已被呼叫。");

            // 嘗試從表單中獲取 CustomField1 (假設儲存了 orderId) 或 MerchantTradeNo
            string orderIdFromCustomField = form.TryGetValue("CustomField1", out var cf1Val) ? cf1Val.ToString() : null;
            string merchantTradeNoFromForm = form.TryGetValue("MerchantTradeNo", out var mtnVal) ? mtnVal.ToString() : null;
            int orderIdForRedirect = 0;

            if (!string.IsNullOrEmpty(orderIdFromCustomField) && int.TryParse(orderIdFromCustomField, out int parsedOrderId))
            {
                orderIdForRedirect = parsedOrderId;
            }
            else if (!string.IsNullOrEmpty(merchantTradeNoFromForm))
            {
                // 如果只有 MerchantTradeNo，你可能需要一個快速查詢來獲取 OrderId (如果前端頁面強烈依賴 OrderId)
                // 但更簡單的做法是讓前端結果頁面能同時接受 OrderId 或 MerchantTradeNo
                _logger.LogInformation($"ECPayOrderResult: 收到 MerchantTradeNo '{merchantTradeNoFromForm}' 但 CustomField1 (OrderId) 為空或無效。");
                // 此處可以選擇是否根據 MTN 去資料庫反查 OrderID，或直接將 MTN 傳給前端。
                // 為簡化，假設前端能處理 MTN，或者 GetFrontendRedirectUrlAfterPayment 內部會處理。
            }
            else
            {
                _logger.LogWarning("ECPayOrderResult: 無法從 Form 中獲取 OrderId (CustomField1) 或 MerchantTradeNo。");
                // 導向一個通用的錯誤頁面
                return Redirect(_ecpayConfig.FrontendFailureUrl + $"?error=missing_order_identifier_client");
            }


            _logger.LogInformation($"ECPayOrderResult - Raw POST Form Data: {string.Join("&", form.Select(kv => $"{kv.Key}={kv.Value}"))}");
            try
            {
                // 將整個 form 傳遞給 Service，由 Service 處理 CheckMacValue (可選) 和產生重導向 URL
                // 注意：orderIdForRedirect 是從 CustomField1 解析的，如果您的 Service 需要它，則傳入。
                // 如果 GetFrontendRedirectUrlAfterPayment 僅依賴 form 內容，則 orderIdForRedirect 可能非必需。
                string redirectUrl = await _ecpayService.GetFrontendRedirectUrlAfterPayment(form, orderIdForRedirect); // 傳入解析出的 orderId

                _logger.LogInformation($"GetFrontendRedirectUrlAfterPayment 返回的重導向 URL: {redirectUrl}");

                if (string.IsNullOrEmpty(redirectUrl))
                {
                    _logger.LogError($"GetFrontendRedirectUrlAfterPayment 為訂單 (解析ID: {orderIdForRedirect}, MTN: {merchantTradeNoFromForm}) 返回了空或無效的重導向 URL。導向預設失敗頁。");
                    return Redirect(_ecpayConfig.FrontendFailureUrl + $"?orderId={orderIdForRedirect}&mtn={merchantTradeNoFromForm}&error=internal_redirect_error");
                }
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"處理訂單 (解析ID: {orderIdForRedirect}, MTN: {merchantTradeNoFromForm}) 的 ECPayOrderResult 時發生錯誤。");
                return Redirect(_ecpayConfig.FrontendFailureUrl + $"?orderId={orderIdForRedirect}&mtn={merchantTradeNoFromForm}&error=exception_client_return");
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
