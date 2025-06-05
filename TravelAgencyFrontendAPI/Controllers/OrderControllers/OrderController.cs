
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyFrontendAPI.DTOs.OrderDTOs;
using TravelAgencyFrontendAPI.ECPay.Services;
using TravelAgencyFrontendAPI.ECPay.Models;

namespace TravelAgencyFrontendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderController> _logger;
        private readonly ECPayService _ecpayService;

        public OrderController(AppDbContext context, ILogger<OrderController> logger, ECPayService ecpayService)
        {
            _context = context;
            _logger = logger;
            _ecpayService = ecpayService;
        }

        // POST: api/Order/initiate
        // << 初步訂單建立」 >>
        /// <summary>
        /// 步驟1：建立初步訂單 (不含付款方式和發票資訊)
        /// </summary>
        /// <param name="orderCreateDto">訂單建立資訊 (不含付款和發票)</param>
        /// <returns>初步訂單摘要，包含OrderId, MerchantTradeNo, ExpiresAt</returns>
        [HttpPost("initiate")]
        public async Task<ActionResult<OrderSummaryDto>> InitiateOrderAsync([FromBody] OrderCreateDto orderCreateDto)
        {
            _logger.LogInformation("接收到初步訂單建立請求: {@OrderCreateDto}", orderCreateDto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("初步訂單模型驗證失敗: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            var memberIdFromDto = orderCreateDto.MemberId;
            var member = await _context.Members.FirstOrDefaultAsync(m => m.MemberId == memberIdFromDto);
            if (member == null)
            {
                _logger.LogWarning("CreateOrder 找不到會員，會員 ID (來自前端DTO): {MemberId}", memberIdFromDto);
                ModelState.AddModelError(nameof(orderCreateDto.MemberId), $"提供的會員 ID {memberIdFromDto} 不存在。");
                return BadRequest(ModelState);
            }

            string tempGuidPart = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            string timePartForMtn = DateTime.Now.ToString("yyMMddHHmmss");
            string prefixForMtn = "TRVORD";
            string mtnBase = $"{prefixForMtn}{timePartForMtn}{tempGuidPart}";
            string merchantTradeNo = new string(mtnBase.Where(char.IsLetterOrDigit).ToArray());
            if (merchantTradeNo.Length > 20)
            {
                merchantTradeNo = merchantTradeNo.Substring(0, 20);
            }
            _logger.LogInformation("為訂單產生的 MerchantTradeNo: {MerchantTradeNo}", merchantTradeNo);

            var order = new Order
            {
                MemberId = member.MemberId,
                OrdererName = orderCreateDto.OrdererInfo.Name,
                OrdererPhone = NormalizePhoneNumber(orderCreateDto.OrdererInfo.MobilePhone),
                OrdererEmail = orderCreateDto.OrdererInfo.Email,
                OrdererNationality = orderCreateDto.OrdererInfo.Nationality,
                OrdererDocumentNumber = orderCreateDto.OrdererInfo.DocumentNumber,
                OrdererDocumentType = orderCreateDto.OrdererInfo.DocumentType,
                Status = OrderStatus.Awaiting,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(3),
                MerchantTradeNo = merchantTradeNo,
                Note = orderCreateDto.OrderNotes,
                OrderDetails = new List<OrderDetail>(),
                OrderParticipants = new List<OrderParticipant>()
            };

            if (orderCreateDto.CartItems == null || !orderCreateDto.CartItems.Any())
            {
                ModelState.AddModelError("CartItems", "購物車中沒有商品。");
                return BadRequest(ModelState);
            }

            int participantDtoGlobalIndex = 0;

            // --- 【核心處理迴圈：建立訂單明細與旅客】 ---
            foreach (var cartItemDto in orderCreateDto.CartItems)
            {
                if (!Enum.TryParse<ProductCategory>(cartItemDto.ProductType, true, out var itemCategory))
                {
                    return BadRequest($"不支援的商品類型: {cartItemDto.ProductType}。");
                }

                if (itemCategory == ProductCategory.GroupTravel)
                {
                    var groupTravel = await _context.GroupTravels.Include(gt => gt.OfficialTravelDetail).ThenInclude(otd => otd.OfficialTravel)
                        .AsNoTracking().FirstOrDefaultAsync(gt => gt.GroupTravelId == cartItemDto.ProductId && gt.GroupStatus == "可報名");

                    if (groupTravel?.OfficialTravelDetail?.OfficialTravel == null || groupTravel.OfficialTravelDetail.State != DetailState.Locked)
                        return BadRequest($"團體行程 (ID: GT{cartItemDto.ProductId}) 資訊不完整或未鎖定價格。");

                    var priceDetail = groupTravel.OfficialTravelDetail;
                    decimal unitPrice = cartItemDto.OptionType.ToUpper() switch
                    {
                        "成人" or "ADULT" => priceDetail.AdultPrice ?? 0,
                        "兒童加床" => priceDetail.AdultPrice ?? 0, // 假設價格
                        "兒童佔床" => priceDetail.ChildPrice ?? 0,
                        "兒童不佔床" => priceDetail.BabyPrice ?? 0, // 假設價格
                        "兒童" or "CHILD" => priceDetail.ChildPrice ?? 0,
                        "嬰兒" or "BABY" => priceDetail.BabyPrice ?? 0,
                        _ => 0
                    };
                    if (unitPrice <= 0 && cartItemDto.Quantity > 0) return BadRequest($"商品 '{groupTravel.OfficialTravelDetail.OfficialTravel.Title}' 的選項 '{cartItemDto.OptionType}' 價格異常。");

                    var groupOrderDetail = new OrderDetail
                    {
                        Order = order,
                        Category = itemCategory,
                        ItemId = cartItemDto.ProductId,
                        Description = $"{groupTravel.OfficialTravelDetail.OfficialTravel.Title} - {cartItemDto.OptionType}",
                        Quantity = cartItemDto.Quantity,
                        Price = unitPrice,
                        TotalAmount = cartItemDto.Quantity * unitPrice,
                        StartDate = groupTravel.DepartureDate,
                        EndDate = groupTravel.ReturnDate,
                        OrderParticipants = new List<OrderParticipant>()
                    };

                    for (int i = 0; i < groupOrderDetail.Quantity; i++)
                    {
                        if (participantDtoGlobalIndex >= orderCreateDto.Participants.Count) return BadRequest("旅客資料數量不足 (團體旅遊)。");
                        var participantDto = orderCreateDto.Participants[participantDtoGlobalIndex++];
                        var participant = MapParticipantFromDto(participantDto, order, groupOrderDetail); // 使用輔助方法
                                                                                                          // 可選：在這裡加入常用旅客資料填充邏輯
                        groupOrderDetail.OrderParticipants.Add(participant);
                        order.OrderParticipants.Add(participant);
                    }
                    order.OrderDetails.Add(groupOrderDetail);
                }
                else if (itemCategory == ProductCategory.CustomTravel)
                {
                    var customTravel = await _context.CustomTravels.AsNoTracking().FirstOrDefaultAsync(ct => ct.CustomTravelId == cartItemDto.ProductId && ct.Status == CustomTravelStatus.Approved);
                    if (customTravel == null) return BadRequest($"客製化行程 (ID: CT{cartItemDto.ProductId}) 無效或狀態不符。");
                    if (customTravel.People <= 0) return BadRequest("客製化行程人數必須大於 0。");

                    decimal perPersonPrice = customTravel.TotalAmount / customTravel.People;

                    for (int i = 0; i < customTravel.People; i++)
                    {
                        if (participantDtoGlobalIndex >= orderCreateDto.Participants.Count) return BadRequest("旅客資料數量與客製化行程人數不符。");

                        var customOrderDetail = new OrderDetail
                        {
                            Order = order,
                            Category = itemCategory,
                            ItemId = cartItemDto.ProductId,
                            Description = $"{customTravel.Note ?? "客製化行程"} - {cartItemDto.OptionType}",
                            Quantity = 1,
                            Price = perPersonPrice,
                            TotalAmount = perPersonPrice,
                            StartDate = customTravel.DepartureDate,
                            EndDate = customTravel.EndDate,
                            OrderParticipants = new List<OrderParticipant>()
                        };

                        var participantDto = orderCreateDto.Participants[participantDtoGlobalIndex++];
                        var participant = MapParticipantFromDto(participantDto, order, customOrderDetail); // 使用輔助方法
                                                                                                           // 可選：在這裡加入常用旅客資料填充邏輯

                        customOrderDetail.OrderParticipants.Add(participant);
                        order.OrderDetails.Add(customOrderDetail);
                        order.OrderParticipants.Add(participant);
                    }
                }
            }

            //【重點】舊的、多餘的旅客分配迴圈已被移除，所有邏輯都在上方完成。

            order.TotalAmount = order.OrderDetails.Sum(d => d.TotalAmount);

            // 在儲存前最後一次驗證 ModelState
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("CreateOrder 最終 ModelState 無效: {@ModelStateErrors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            // 資料庫儲存 (使用交易)
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    // 處理常用旅客的更新或新增
                    if (orderCreateDto.TravelerProfileActions != null && orderCreateDto.TravelerProfileActions.Any())
                    {
                        // ... 此處是您原本的 TravelerProfileActions 邏輯，保持不變 ...
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "建立訂單時發生錯誤。");
                    return StatusCode(StatusCodes.Status500InternalServerError, "建立訂單時發生錯誤。");
                }
            }

            var orderSummary = new OrderSummaryDto
            {
                OrderId = order.OrderId,
                MerchantTradeNo = order.MerchantTradeNo,
                OrderStatus = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                ExpiresAt = order.ExpiresAt
            };
            _logger.LogInformation("初步訂單建立成功，回傳 orderSummary: {@OrderSummary}", orderSummary);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId, memberId = order.MemberId }, orderSummary);
        }

        // 建議新增一個輔助方法來映射 Participant，讓主方法更乾淨
        private OrderParticipant MapParticipantFromDto(OrderParticipantDto dto, Order order, OrderDetail detail)
        {
            // 在這裡可以加入您對 participantDto 的驗證邏輯
            if (dto.DocumentType == DocumentType.PASSPORT && string.IsNullOrEmpty(dto.DocumentNumber))
            {
                ModelState.AddModelError($"Participants[{dto.Name}].DocumentNumber", "選擇護照作為證件類型時，護照號碼為必填。");
            }
            if (string.IsNullOrEmpty(dto.IdNumber) && string.IsNullOrEmpty(dto.DocumentNumber))
            {
                ModelState.AddModelError($"Participants[{dto.Name}].IdOrDocumentNumber", "旅客身分證號或證件號碼至少需填寫一項。");
            }

            return new OrderParticipant
            {
                Order = order,
                OrderDetail = detail,
                Name = dto.Name,
                BirthDate = dto.BirthDate,
                IdNumber = dto.IdNumber,
                Gender = dto.Gender,
                DocumentType = dto.DocumentType,
                DocumentNumber = dto.DocumentNumber,
                PassportSurname = dto.PassportSurname,
                PassportGivenName = dto.PassportGivenName,
                PassportExpireDate = dto.PassportExpireDate,
                Nationality = dto.Nationality,
                Note = dto.Note,
                FavoriteTravelerId = dto.FavoriteTravelerId
            };
        }
        // << 處理第二階段付款與發票資訊的 Action >>
        /// <summary>
        /// 步驟2：最終確認訂單付款方式與發票資訊，並取得 ECPay 付款參數
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <param name="paymentDto">付款及發票資訊</param>
        /// <returns>ECPay 付款表單所需參數</returns>
        [HttpPut("{orderId}/finalize-payment")]
        public async Task<ActionResult<ECPayService.ECPayPaymentRequestViewModel>> FinalizePaymentAsync(int orderId, [FromBody] OrderPaymentFinalizationDto paymentDto)
        {
            _logger.LogInformation("接收到訂單最終確認付款請求，OrderID: {OrderId}, DTO: {@PaymentDto}", orderId, paymentDto); // << 新增：日誌記錄 >>

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("訂單最終確認付款模型驗證失敗: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            if (paymentDto.MemberId <= 0)
            {
                _logger.LogWarning("訂單最終確認付款：DTO 中提供的 MemberId 無效: {ProvidedMemberId}", paymentDto.MemberId);
                return BadRequest(new { message = "請求中缺少有效的會員ID。" });
            }
            var memberIdFromDto = paymentDto.MemberId;
            var order = await _context.Orders
                                .Include(o => o.OrderDetails) // ECPayService 可能會用到
                                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.MemberId == memberIdFromDto);

            if (order == null)
            {
                _logger.LogWarning("訂單最終確認付款：找不到訂單 ID {OrderId} 或會員 {MemberIdFromDto} 無權限。", orderId, memberIdFromDto);
                return NotFound(new { message = $"找不到訂單 {orderId} 或您無權存取。" });
            }

            if (order.Status != OrderStatus.Awaiting)
            {
                _logger.LogWarning("訂單 {OrderId} 狀態為 {Status}，無法進行付款設定。", orderId, order.Status);
                return BadRequest(new { message = $"訂單目前狀態為 '{order.Status}'，無法設定付款資訊。請確認訂單狀態或重新下單。" });
            }

            // 更新訂單的付款方式和發票資訊
            order.PaymentMethod = paymentDto.SelectedPaymentMethod;
            order.InvoiceOption = paymentDto.InvoiceRequestInfo.InvoiceOption;
            order.InvoiceDeliveryEmail = paymentDto.InvoiceRequestInfo.InvoiceDeliveryEmail;
            order.InvoiceUniformNumber = paymentDto.InvoiceRequestInfo.InvoiceUniformNumber;
            order.InvoiceTitle = paymentDto.InvoiceRequestInfo.InvoiceTitle;
            order.InvoiceAddBillingAddr = paymentDto.InvoiceRequestInfo.InvoiceAddBillingAddr;
            order.InvoiceBillingAddress = paymentDto.InvoiceRequestInfo.InvoiceBillingAddress;
            // 如果 OrderPaymentFinalizationDto.InvoiceRequestInfo 有其他發票欄位，一併更新

            try
            {
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                _logger.LogInformation("訂單 {OrderId} 已更新付款及發票資訊。", order.OrderId);

                // << 新增：調用 ECPayService 來產生綠界付款表單參數 >>
                // 注意：ECPayService.GenerateEcPayPaymentForm 內部會再次檢查訂單狀態和效期
                var ecpayViewModel = await _ecpayService.GenerateEcPayPaymentForm(order.OrderId);

                _logger.LogInformation("成功為訂單 {OrderId} 產生 ECPay 付款參數，準備回傳前端。", order.OrderId);
                return Ok(ecpayViewModel);
            }
            catch (InvalidOperationException ex) // 可能由 ECPayService 拋出 (例如訂單狀態不符或已過期)
            {
                _logger.LogWarning(ex, "準備 ECPay 參數時發生業務邏輯錯誤 (InvalidOperationException)。訂單 ID: {OrderId}", orderId);
                return BadRequest(new { message = ex.Message }); // 將 ECPayService 的錯誤訊息回傳給前端
            }
            catch (ArgumentException ex) // 可能由 ECPayService 拋出 (例如找不到訂單)
            {
                _logger.LogWarning(ex, "準備 ECPay 參數時發生參數錯誤 (ArgumentException)。訂單 ID: {OrderId}", orderId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "最終確認付款並產生 ECPay 參數時發生未預期錯誤。訂單 ID: {OrderId}", orderId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "處理付款請求時發生內部錯誤，請稍後再試。" });
            }
        }

        // << 新增 API 端點：啟動訂單的短時效期 (30秒猶豫期) >>
        // POST: api/Order/{orderId}/activate-short-expiration?memberId=123
        [HttpPost("{orderId}/activate-short-expiration")]
        public async Task<IActionResult> ActivateShortExpiration(int orderId, [FromQuery] int? memberId)
        {
            _logger.LogInformation("[ActivateShortExpiration] API endpoint hit for OrderId: {OrderId}, MemberId from query: {QueryMemberId}", orderId, memberId);

            if (memberId == null || memberId.Value <= 0)
            {
                _logger.LogWarning("[ActivateShortExpiration] Invalid memberId received: {QueryMemberId}", memberId);
                return BadRequest(new { message = "需要提供有效的 memberId。" });
            }

            // << 修改：確認您的 Order 模型主鍵是 OrderId 還是 Id >>
            var order = await _context.Orders
                                    .FirstOrDefaultAsync(o => o.OrderId == orderId && o.MemberId == memberId.Value);

            if (order == null)
            {
                _logger.LogWarning("[ActivateShortExpiration] Order not found. OrderId: {OrderId}, MemberId: {MemberId}", orderId, memberId.Value);
                return NotFound(new { message = $"找不到訂單 (ID: {orderId}) 或該訂單不屬於此會員。" });
            }

            // 只有狀態為 Awaiting 的訂單才能啟動短時效期 (猶豫期)
            // 並且，只有當目前的 ExpiresAt 不是一個非常短的時間 (例如，如果已經是猶豫期，則不再重複設定)
            // 但簡化起見，只要是 Awaiting 就允許重設為 30 秒猶豫期
            if (order.Status == OrderStatus.Awaiting)
            {
                var newExpiresAt = DateTime.Now.AddMinutes(3);
                order.ExpiresAt = newExpiresAt; // 更新 ExpiresAt 為3分鐘後

                // 可選：記錄操作到訂單備註
                // string hesitationNote = $"猶豫期啟動於 {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC，新的到期時間: {newExpiresAt:yyyy-MM-dd HH:mm:ss} UTC。";
                // order.Note = string.IsNullOrEmpty(order.Note) ? hesitationNote : $"{order.Note}\n{hesitationNote}";

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("[ActivateShortExpiration] Order {OrderId} short expiration activated. New ExpiresAt: {NewExpiresAt} UTC.", order.OrderId, newExpiresAt);
                    // 返回新的到期時間，前端可以選擇性使用
                    return Ok(new { message = "訂單猶豫期已啟動 (3分鐘)。", expiresAt = newExpiresAt });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[ActivateShortExpiration] 更新訂單 {OrderId} ExpiresAt 時發生錯誤。", order.OrderId);
                    return StatusCode(500, "更新訂單到期時間時發生內部錯誤。");
                }
            }
            else
            {
                _logger.LogWarning("[ActivateShortExpiration] Order {OrderId} 狀態為 {OrderStatus}，不適用於啟動猶豫期。", order.OrderId, order.Status);
                return BadRequest(new { message = $"訂單狀態 ({order.Status}) 不允許啟動猶豫期。" });
            }
        }

        // *** 在這裡添加電話號碼正規化的私有 Helper Method ***
        private string NormalizePhoneNumber(string phoneNumberString)
        {
            if (string.IsNullOrWhiteSpace(phoneNumberString)) // 使用 IsNullOrWhiteSpace 更佳
            {
                return phoneNumberString; // 如果是 null 或空字串，直接返回
            }

            string normalizedNumber = phoneNumberString.Trim(); // 移除前後空白

            // 檢查是否以 "+" 開頭，且長度足夠進行檢查 (至少包含 +國碼數字0數字 或 +國碼數字數字)
            if (normalizedNumber.StartsWith("+"))
            {
                // 移除開頭的 '+'
                string numberWithoutPlus = normalizedNumber.Substring(1);

                // 特別處理台灣的國碼 "886"
                if (numberWithoutPlus.StartsWith("886"))
                {
                    string countryCodePart = "886";
                    // 取得 "886" 後面的號碼部分
                    string nationalNumberPart = numberWithoutPlus.Substring(countryCodePart.Length); // 例如 "0905088127" 或 "905088127"

                    // 如果號碼部分以 "0" 開頭，且長度大於1 (例如 "09..." 而非只有 "0")
                    if (nationalNumberPart.StartsWith("0") && nationalNumberPart.Length > 1)
                    {
                        nationalNumberPart = nationalNumberPart.Substring(1); // 移除開頭的 "0"，變成 "905088127"
                    }
                    // 組合國碼和處理後的號碼部分
                    _logger.LogInformation("Normalized phone from {Original} to {Processed}", phoneNumberString, countryCodePart + nationalNumberPart);
                    return countryCodePart + nationalNumberPart; // 結果："886905088127"
                }
                else
                {
                    // 警告：這假設 "+" 後面緊跟的是完整的數字，如果包含其他非數字字元，可能需要額外處理。
                    if (numberWithoutPlus.All(char.IsDigit))
                    {
                        _logger.LogInformation("Normalized non-TW phone from {Original} to {Processed} (removed '+')", phoneNumberString, numberWithoutPlus);
                        return numberWithoutPlus; // 例如：輸入 "+14155552671"，返回 "14155552671"
                    }
                    else
                    {
                        _logger.LogWarning("Phone number {PhoneNumber} starts with '+' but is not in the expected +886... format or contains non-digits after the initial '+'. Returning without '+'.", phoneNumberString);
                        // 嘗試移除 "+" 後，移除所有非數字字元 (這是一個比較寬鬆的處理)
                        // string cleanedNumber = new string(numberWithoutPlus.Where(char.IsDigit).ToArray());
                        // return cleanedNumber;
                        // 或者，如果格式不符預期，可以考慮返回原始號碼(不含+)或拋出格式錯誤
                        return numberWithoutPlus; // 目前僅移除 '+'
                    }
                }
            }
            _logger.LogInformation("Phone number {Original} did not match specific normalization rules, returning trimmed version.", phoneNumberString);
            return normalizedNumber;
        }

        // 輔助方法：獲取當前登入使用者的 MemberId
        // 實際實作會根據您的身份驗證設定而有所不同
        private int? GetCurrentUserId()
        {
            // ... (您的用戶ID獲取邏輯) ...
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim, out int userId)) { return userId; }
                _logger.LogWarning("GetCurrentUserId (parameterless): 已認證，但無法從 Claims 中解析 UserId。ClaimValue: '{ClaimValue}'", userIdClaim);
            }
            else { _logger.LogWarning("GetCurrentUserId (parameterless): User 未認證或 User.Identity 為 null。"); }
            return null;
        }

        /// <summary>
        /// 查詢特定訂單的最新狀態，供前端核對。
        /// </summary>
        /// <param name="orderId">要查詢的訂單 ID</param>
        /// <returns>PendingOrderStatusDto 或 NotFound/Unauthorized</returns>
        [HttpGet("{orderId}/status-check")] // 例如: GET /api/Order/123/status-check
        public async Task<ActionResult<PendingOrderStatusDto>> CheckOrderStatus(int orderId)
        {
            var memberId = GetCurrentUserId();
            if (memberId == null)
            {
                _logger.LogWarning("CheckOrderStatus for OrderId {OrderId}: User 未認證或無法獲取 MemberId。", orderId);
                return Unauthorized(new { message = "使用者未登入或無法識別身份。" });
            }

            var orderStatusInfo = await _context.Orders
                .Where(o => o.OrderId == orderId && o.MemberId == memberId.Value) // 確保是該會員的訂單
                .Select(o => new PendingOrderStatusDto
                {
                    OrderId = o.OrderId,
                    MerchantTradeNo = o.MerchantTradeNo,
                    Status = o.Status.ToString(),
                    ExpiresAt = o.ExpiresAt, // 即使過期也回傳，讓前端知道
                    TotalAmount = o.TotalAmount,
                    PaymentMethod = o.PaymentMethod.ToString()
                })
                .FirstOrDefaultAsync();

            if (orderStatusInfo == null)
            {
                _logger.LogWarning("CheckOrderStatus: MemberId {MemberId} 查詢不到訂單 {OrderId} 或無權限。", memberId.Value, orderId);
                return NotFound(new { message = $"找不到訂單 {orderId} 或您無權存取。" });
            }
            _logger.LogInformation("CheckOrderStatus: 查詢到訂單狀態 {@OrderStatusInfo} for OrderId {OrderId}, MemberId {MemberId}", orderStatusInfo, orderId, memberId.Value);
            return Ok(orderStatusInfo);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id, [FromQuery] int? memberId)
        {
            if (memberId == null || memberId.Value <= 0)
            {
                _logger.LogWarning("GetOrderById 請求中未提供有效的 memberId。訂單ID: {OrderId}", id);
                // 返回 BadRequest，告知前端需要提供 memberId
                return BadRequest("查詢訂單詳情需要提供有效的 memberId。");
            }

            var orderData = await _context.Orders
                .Include(o => o.OrderParticipants) // 如果 OrderParticipant 也需要
                .Include(o => o.OrderDetails)
                //    .ThenInclude(od => od.GroupTravel) // 預先載入 GroupTravel
                //        .ThenInclude(gt => gt.OfficialTravelDetail) // 如果需要更深層的
                //            .ThenInclude(otd => otd.OfficialTravel) // 同上
                //.Include(o => o.OrderDetails)
                //    .ThenInclude(od => od.CustomTravel) // 預先載入 CustomTravel
                .FirstOrDefaultAsync(o => o.OrderId == id && o.MemberId == memberId.Value);

            if (orderData == null)
            {
                return NotFound($"找不到訂單 ID {id}，或您無權存取此訂單。");
            }

            // 手動載入關聯的 GroupTravel 和 CustomTravel (如果需要且未在上面 Include)
            // 這種方式效率可能稍差，但較直觀
            // --- 核心修改：手動載入 OrderDetail 中的 GroupTravel 或 CustomTravel ---
            if (orderData.OrderDetails != null) // 確保 OrderDetails 不是 null
            {
                foreach (var detail in orderData.OrderDetails)
                {
                    if (detail.Category == ProductCategory.GroupTravel)
                    {
                        // 使用 ItemId 從 _context.GroupTravels 查詢
                        // 確保你GroupTravel 實體有與 detail.ItemId 對應的主鍵，例如 GroupTravelId
                        detail.GroupTravel = await _context.GroupTravels
                            .Include(gt => gt.OfficialTravelDetail)
                                .ThenInclude(otd => otd.OfficialTravel)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(gt => gt.GroupTravelId == detail.ItemId);
                    }
                    else if (detail.Category == ProductCategory.CustomTravel)
                    {
                        detail.CustomTravel = await _context.CustomTravels
                            .AsNoTracking()
                            .FirstOrDefaultAsync(ct => ct.CustomTravelId == detail.ItemId);
                    }
                }
            }
            var orderDto = new // 你應該用你定義的 OrderDto
            {
                OrderId = orderData.OrderId,
                MemberId = orderData.MemberId,
                TotalAmount = orderData.TotalAmount,
                PaymentMethod = orderData.PaymentMethod.ToString(),
                OrderStatus = orderData.Status.ToString(),
                CreatedAt = orderData.CreatedAt,
                PaymentDate = orderData.PaymentDate,
                Note = orderData.Note,
                InvoiceOption = orderData.InvoiceOption.ToString(), // 轉換為字串
                InvoiceDeliveryEmail = orderData.InvoiceDeliveryEmail,
                InvoiceUniformNumber = orderData.InvoiceUniformNumber,
                InvoiceTitle = orderData.InvoiceTitle,
                InvoiceAddBillingAddr = orderData.InvoiceAddBillingAddr,
                InvoiceBillingAddress = orderData.InvoiceBillingAddress,
                ExpiresAt = orderData.ExpiresAt, // 回傳訂單過期時間 
                MerchantTradeNo = orderData.MerchantTradeNo,// 回傳商店交易編號 

                OrdererInfo = new OrdererInfoDto
                {
                    Name = orderData.OrdererName,
                    MobilePhone = orderData.OrdererPhone,
                    Email = orderData.OrdererEmail,
                    Nationality = orderData.OrdererNationality,
                    DocumentType = orderData.OrdererDocumentType.ToString(),
                    DocumentNumber = orderData.OrdererDocumentNumber
                },

                Participants = orderData.OrderParticipants?.Select(p => new OrderParticipantDto
                {
                    OrderDetailId = p.OrderDetailId,
                    Name = p.Name,
                    BirthDate = p.BirthDate,
                    IdNumber = p.IdNumber,
                    Gender = p.Gender, // 轉換為字串
                    DocumentType = p.DocumentType, // 轉換為字串
                    DocumentNumber = p.DocumentNumber,
                    PassportSurname = p.PassportSurname,
                    PassportGivenName = p.PassportGivenName,
                    PassportExpireDate = p.PassportExpireDate,
                    Nationality = p.Nationality,
                    Note = p.Note,
                    FavoriteTravelerId = p.FavoriteTravelerId
                }).ToList() ?? new List<OrderParticipantDto>(),

                OrderDetails = orderData.OrderDetails?.Select(od =>
                {
                    string productTitle = string.Empty;
                    //DateTime? itemStartDate = null;
                    //DateTime? itemEndDate = null;
                    if (od.Category == ProductCategory.GroupTravel && od.GroupTravel != null)
                    {
                        productTitle = od.GroupTravel.OfficialTravelDetail?.OfficialTravel?.Title ?? string.Empty;
                        // 假設 OfficialTravelDetail 或 GroupTravel 有 StartDate/EndDate
                        //itemStartDate = od.GroupTravel.DepartureDate;
                        //itemEndDate = od.GroupTravel.ReturnDate;
                    }
                    else if (od.Category == ProductCategory.CustomTravel && od.CustomTravel != null)
                    {
                        productTitle = od.CustomTravel.Note ?? $"客製化行程 {od.CustomTravel.CustomTravelId}";
                        // 假設 CustomTravel 有 StartDate/EndDate
                        //itemStartDate = od.CustomTravel.DepartureDate;
                        //itemEndDate = od.CustomTravel.EndDate;
                    }

                    if (string.IsNullOrEmpty(productTitle) && !string.IsNullOrEmpty(od.Description))
                    {
                        int separatorIndex = od.Description.LastIndexOf(" - ");
                        productTitle = separatorIndex > 0 ? od.Description.Substring(0, separatorIndex) : od.Description;
                    }


                    return new OrderDetailItemDto
                    {
                        OrderDetailId = od.OrderDetailId,
                        Category = od.Category,
                        ItemId = od.ItemId,
                        Description = od.Description,
                        Quantity = od.Quantity,
                        Price = od.Price,
                        TotalAmount = od.TotalAmount,
                        ProductType = od.Category.ToString(),
                        OptionType = ParseOptionTypeFromDescription(od.Description, productTitle),
                        StartDate = od.StartDate,
                        EndDate = od.EndDate,
                    };
                }).ToList() ?? new List<OrderDetailItemDto>(),
            };

            return Ok(orderDto);
        }
        // 輔助方法：從 OrderDetail.Description 中解析 OptionType (這部分比較tricky，需要你根據 Description 的格式來實作)
        private string ParseOptionTypeFromDescription(string description, string productName)
        {
            if (string.IsNullOrEmpty(description) || string.IsNullOrEmpty(productName)) return description; // 或返回空字串
            // 假設 Description 格式為 "{productName} - {optionSpecificDescription}"
            string prefix = $"{productName} - ";
            if (description.StartsWith(prefix))
            {
                return description.Substring(prefix.Length);
            }
            return description; // 如果格式不符，返回原始描述或特定預設值
        }

        [HttpGet("{orderId}/invoice")] // 例如: GET /api/Order/149/invoice
        public async Task<ActionResult<InvoiceDetailsDto>> GetInvoiceDetails(int orderId, [FromQuery] int? memberId)
        {
            // 1. 安全性檢查 (類似 GetOrderById，確認 memberId 與訂單匹配)
            if (memberId == null || memberId.Value <= 0)
            {
                return BadRequest("查詢發票詳情需要提供有效的 memberId。");
            }

            var order = await _context.Orders
                                      .Include(o => o.OrderInvoices) // 確保載入已存在的發票記錄
                                      .FirstOrDefaultAsync(o => o.OrderId == orderId && o.MemberId == memberId.Value);

            if (order == null)
            {
                return NotFound($"找不到訂單 ID {orderId}，或您無權存取此訂單。");
            }

            // 2. 查找已開立的發票記錄
            var openedInvoice = order.OrderInvoices
                                         .FirstOrDefault(inv => inv.InvoiceStatus == InvoiceStatus.Opened); // 假設 Opened 表示成功

            if (openedInvoice != null)
            {
                return Ok(new InvoiceDetailsDto
                {
                    RtnCode = 1,
                    RtnMsg = "發票已成功開立。",
                    InvoiceNo = openedInvoice.InvoiceNumber,
                    InvoiceDate = openedInvoice.CreatedAt.ToString("yyyy/MM/dd HH:mm:ss"), // 使用一致的日期格式
                    RandomNumber = openedInvoice.RandomCode,

                    InvoiceType = openedInvoice.InvoiceType.ToString(), // 將 enum 轉為字串
                    BuyerName = openedInvoice.BuyerName,
                    BuyerUniformNumber = openedInvoice.BuyerUniformNumber,
                    TotalAmount = openedInvoice.TotalAmount,
                    InvoiceItemDesc = openedInvoice.InvoiceItemDesc,
                    Note = openedInvoice.Note
                    // InvoiceStatus = openedInvoice.InvoiceStatus.ToString() // 如果也想回傳狀態
                });
            }

            // 3. 如果資料庫中沒有已開立的發票記錄，且訂單已付款，可以考慮是否要嘗試即時開立或提示
            //    這裡的邏輯取決於您的業務流程：
            //    a) 付款成功時 (ECPay callback) 就應該已觸發 IssueInvoiceAsync。
            //    b) 此處僅為查詢，若未開立則提示。
            //    c) 此處嘗試補開 (較複雜，需注意重複開票問題)。

            // 假設流程 (a) 已完成，這裡主要做查詢
            // 如果在 ECPay callback 中開票失敗，OrderInvoice 中可能會有失敗記錄
            var failedInvoiceAttempt = order.OrderInvoices.OrderByDescending(i => i.CreatedAt).FirstOrDefault();
            if (failedInvoiceAttempt != null && failedInvoiceAttempt.InvoiceStatus != InvoiceStatus.Opened)
            {
                _logger.LogWarning("訂單 {OrderId} 先前嘗試開立發票但未成功: {Note}", orderId, failedInvoiceAttempt.Note);
                return Ok(new InvoiceDetailsDto
                {
                    RtnCode = 0, // 自定義失敗碼
                    RtnMsg = $"發票先前嘗試開立但未成功: {failedInvoiceAttempt.Note ?? "請洽客服"}",
                });
            }


            _logger.LogInformation("訂單 {OrderId} 尚未找到已開立的發票記錄。", orderId);
            return Ok(new InvoiceDetailsDto
            {
                RtnCode = 2, // 自定義碼，表示尚未開立或查詢不到
                RtnMsg = "此訂單目前尚無已開立的發票資訊。"
            });
        }

        [HttpPut("{orderId}")]
        public async Task<ActionResult<OrderSummaryDto>> UpdateOrderAsync(int orderId, [FromBody] OrderUpdateDto orderUpdateDto)
        {
            _logger.LogInformation("接收到訂單更新請求，OrderID: {OrderId}, DTO: {@OrderUpdateDto}", orderId, orderUpdateDto);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var order = await _context.Orders
                        .Include(o => o.OrderDetails)
                            .ThenInclude(od => od.OrderParticipants)
                        .FirstOrDefaultAsync(o => o.OrderId == orderId);

                    if (order == null)
                    {
                        return NotFound(new { message = $"找不到訂單 {orderId} 或您無權修改此訂單。" });
                    }

                    if (order.Status != OrderStatus.Awaiting)
                    {
                        return BadRequest(new { message = $"訂單目前狀態為 '{order.Status}'，無法修改。" });
                    }

                    // 1. 更新 Order 的基本資料
                    order.OrdererName = orderUpdateDto.OrdererInfo.Name;
                    order.OrdererPhone = NormalizePhoneNumber(orderUpdateDto.OrdererInfo.MobilePhone);
                    order.OrdererEmail = orderUpdateDto.OrdererInfo.Email;
                    order.OrdererNationality = orderUpdateDto.OrdererInfo.Nationality;
                    order.OrdererDocumentNumber = orderUpdateDto.OrdererInfo.DocumentNumber;
                    order.OrdererDocumentType = orderUpdateDto.OrdererInfo.DocumentType;
                    order.Note = orderUpdateDto.OrderNotes;
                    order.UpdatedAt = DateTime.Now;
                    order.ExpiresAt = DateTime.Now.AddMinutes(3);

                    // 2. 刪除所有舊的 OrderDetails 和關聯的 OrderParticipants
                    if (order.OrderDetails.Any())
                    {
                        var participantsToRemove = order.OrderDetails.SelectMany(od => od.OrderParticipants).ToList();
                        if (participantsToRemove.Any())
                        {
                            _context.OrderParticipants.RemoveRange(participantsToRemove);
                        }
                        _context.OrderDetails.RemoveRange(order.OrderDetails);

                        // 強制 EF Core 執行刪除操作，避免在同一個 transaction 中因主鍵衝突而出錯
                        await _context.SaveChangesAsync();
                    }

                    // 3. 根據 orderUpdateDto 重新創建所有 OrderDetails 和 OrderParticipants
                    int participantDtoGlobalIndex = 0;

                    if (orderUpdateDto.CartItems == null || !orderUpdateDto.CartItems.Any())
                    {
                        return BadRequest(new { message = "更新訂單時，購物車中沒有商品。" });
                    }

                    var distinctCartItems = orderUpdateDto.CartItems
                        .GroupBy(item => new { item.ProductId, item.ProductType })
                        .Select(group => group.First()) // 對於相同的產品，只取第一筆
                        .ToList();

                    // --- 【核心重構區域：與 InitiateOrderAsync 同步的邏輯】 ---
                    foreach (var cartItemDto in distinctCartItems)
                    {
                        if (!Enum.TryParse<ProductCategory>(cartItemDto.ProductType, true, out var itemCategory))
                        {
                            return BadRequest($"不支援的商品類型: {cartItemDto.ProductType}。");
                        }

                        if (itemCategory == ProductCategory.GroupTravel)
                        {
                            var groupTravel = await _context.GroupTravels.Include(gt => gt.OfficialTravelDetail).ThenInclude(otd => otd.OfficialTravel)
                                .AsNoTracking().FirstOrDefaultAsync(gt => gt.GroupTravelId == cartItemDto.ProductId && gt.GroupStatus == "可報名");

                            if (groupTravel?.OfficialTravelDetail?.OfficialTravel == null || groupTravel.OfficialTravelDetail.State != DetailState.Locked)
                                return BadRequest($"團體行程 (ID: GT{cartItemDto.ProductId}) 資訊不完整或未鎖定價格。");

                            var priceDetail = groupTravel.OfficialTravelDetail;
                            decimal unitPrice = cartItemDto.OptionType.ToUpper() switch
                            {
                                "成人" or "ADULT" => priceDetail.AdultPrice ?? 0,
                                "兒童加床" => priceDetail.AdultPrice ?? 0,
                                "兒童佔床" => priceDetail.ChildPrice ?? 0,
                                "兒童不佔床" => priceDetail.BabyPrice ?? 0,
                                "兒童" or "CHILD" => priceDetail.ChildPrice ?? 0,
                                "嬰兒" or "BABY" => priceDetail.BabyPrice ?? 0,
                                _ => 0
                            };
                            if (unitPrice <= 0 && cartItemDto.Quantity > 0) return BadRequest($"商品 '{groupTravel.OfficialTravelDetail.OfficialTravel.Title}' 的選項 '{cartItemDto.OptionType}' 價格異常。");

                            var groupOrderDetail = new OrderDetail
                            {
                                Order = order,
                                Category = itemCategory,
                                ItemId = cartItemDto.ProductId,
                                Description = $"{groupTravel.OfficialTravelDetail.OfficialTravel.Title} - {cartItemDto.OptionType}",
                                Quantity = cartItemDto.Quantity,
                                Price = unitPrice,
                                TotalAmount = cartItemDto.Quantity * unitPrice,
                                StartDate = groupTravel.DepartureDate,
                                EndDate = groupTravel.ReturnDate,
                                OrderParticipants = new List<OrderParticipant>()
                            };

                            for (int i = 0; i < groupOrderDetail.Quantity; i++)
                            {
                                if (participantDtoGlobalIndex >= orderUpdateDto.Participants.Count) return BadRequest("旅客資料數量不足 (團體旅遊)。");
                                var participantDto = orderUpdateDto.Participants[participantDtoGlobalIndex++];
                                var participant = MapParticipantFromDto(participantDto, order, groupOrderDetail);
                                groupOrderDetail.OrderParticipants.Add(participant);
                                order.OrderParticipants.Add(participant);
                            }
                            order.OrderDetails.Add(groupOrderDetail);
                        }
                        else if (itemCategory == ProductCategory.CustomTravel)
                        {
                            var customTravel = await _context.CustomTravels.AsNoTracking().FirstOrDefaultAsync(ct => ct.CustomTravelId == cartItemDto.ProductId && ct.Status == CustomTravelStatus.Approved);
                            if (customTravel == null) return BadRequest($"客製化行程 (ID: CT{cartItemDto.ProductId}) 無效或狀態不符。");
                            if (customTravel.People <= 0) return BadRequest("客製化行程人數必須大於 0。");

                            decimal perPersonPrice = customTravel.TotalAmount / customTravel.People;

                            for (int i = 0; i < customTravel.People; i++)
                            {
                                if (participantDtoGlobalIndex >= orderUpdateDto.Participants.Count) return BadRequest("旅客資料數量與客製化行程人數不符。");

                                var customOrderDetail = new OrderDetail
                                {
                                    Order = order,
                                    Category = itemCategory,
                                    ItemId = cartItemDto.ProductId,
                                    Description = $"{customTravel.Note ?? "客製化行程"} - {cartItemDto.OptionType}",
                                    Quantity = 1,
                                    Price = perPersonPrice,
                                    TotalAmount = perPersonPrice,
                                    StartDate = customTravel.DepartureDate,
                                    EndDate = customTravel.EndDate,
                                    OrderParticipants = new List<OrderParticipant>()
                                };

                                var participantDto = orderUpdateDto.Participants[participantDtoGlobalIndex++];
                                var participant = MapParticipantFromDto(participantDto, order, customOrderDetail);

                                customOrderDetail.OrderParticipants.Add(participant);
                                order.OrderDetails.Add(customOrderDetail);
                                order.OrderParticipants.Add(participant);
                            }
                        }
                    }
                    // --- 【核心重構區域結束】 ---

                    order.TotalAmount = order.OrderDetails.Sum(d => d.TotalAmount);

                    // 4. 處理常用旅客的更新 (如果需要)
                    if (orderUpdateDto.TravelerProfileActions != null && orderUpdateDto.TravelerProfileActions.Any())
                    {
                        // ... 您原本的 TravelerProfileActions 邏輯可以放在這裡 ...
                    }

                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var orderSummary = new OrderSummaryDto
                    {
                        OrderId = order.OrderId,
                        MerchantTradeNo = order.MerchantTradeNo,
                        OrderStatus = order.Status.ToString(),
                        TotalAmount = order.TotalAmount,
                        ExpiresAt = order.ExpiresAt
                    };

                    _logger.LogInformation("訂單 {OrderId} 更新成功，並回傳摘要。", order.OrderId);
                    return Ok(orderSummary);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "更新訂單 {OrderId} 時發生錯誤。", orderId);
                    return StatusCode(StatusCodes.Status500InternalServerError, "更新訂單失敗，請稍後再試。");
                }
            }
        }
    }
}
    