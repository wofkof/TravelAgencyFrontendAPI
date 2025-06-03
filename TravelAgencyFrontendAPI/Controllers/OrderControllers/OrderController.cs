
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

            // << 新增：產生唯一的商店交易編號 (MerchantTradeNo) 移至此處 >>
            // 確保此編號的唯一性，綠界要求20碼內英數字
            string tempGuidPart = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            string timePartForMtn = DateTime.Now.ToString("yyMMddHHmmss"); // 精確到秒，減少重複機率
            string prefixForMtn = "TRVORD"; // 您的商店前綴
            string mtnBase = $"{prefixForMtn}{timePartForMtn}{tempGuidPart}";
            string merchantTradeNo = new string(mtnBase.Where(char.IsLetterOrDigit).ToArray());
            if (merchantTradeNo.Length > 20)
            {
                merchantTradeNo = merchantTradeNo.Substring(0, 20);
            }
            _logger.LogInformation("為訂單產生的 MerchantTradeNo: {MerchantTradeNo}", merchantTradeNo);


            // 初始化 Order 物件
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
                ExpiresAt = DateTime.Now.AddMinutes(3), // 3分鐘後失效 (之後背景服務會處理)
                MerchantTradeNo = merchantTradeNo, // << 新增：設定商店交易編號 >>
                Note = orderCreateDto.OrderNotes,
                OrderDetails = new List<OrderDetail>(),
                OrderParticipants = new List<OrderParticipant>()
            };
            decimal calculatedServerTotalAmount = 0;
            int participantDtoGlobalIndex = 0;
            if (orderCreateDto.CartItems == null || !orderCreateDto.CartItems.Any())
            {
                ModelState.AddModelError("CartItems", "購物車中沒有商品。");
                return BadRequest(ModelState);
            }
            // --- 核心修改：根據前端傳來的 CartItems 生成 OrderDetails ---
            foreach (var cartItemDto in orderCreateDto.CartItems)
            {
                decimal unitPrice = 0;
                string productName = "產品名稱";
                string optionSpecificDescription = cartItemDto.OptionType; // 選項的描述
                ProductCategory itemCategory; // 用來儲存轉換後的 ProductCategory

                DateTime? itemSpecificStartDate = null;
                DateTime? itemSpecificEndDate = null;

                if (!Enum.TryParse<ProductCategory>(cartItemDto.ProductType, true, out var parsedCategory))
                {
                    _logger.LogWarning($"無效的 ProductType: {cartItemDto.ProductType}。");
                    return BadRequest($"不支援的商品類型: {cartItemDto.ProductType}。");
                }
                itemCategory = parsedCategory;

                if (itemCategory == ProductCategory.GroupTravel)
                {
                    var groupTravel = await _context.GroupTravels
                                                .Include(gt => gt.OfficialTravelDetail)
                                                    .ThenInclude(otd => otd.OfficialTravel)
                                                .FirstOrDefaultAsync(gt => gt.GroupTravelId == cartItemDto.ProductId && gt.GroupStatus == "可報名");

                    if (groupTravel == null || groupTravel.OfficialTravelDetail == null || groupTravel.OfficialTravelDetail.OfficialTravel == null || groupTravel.OfficialTravelDetail.State != DetailState.Locked)
                    { 
                        return BadRequest($"團體行程 (ID: GT{cartItemDto.ProductId}) 資訊不完整或未鎖定價格。"); 
                    }

                    productName = groupTravel.OfficialTravelDetail.OfficialTravel.Title;
                    var priceDetail = groupTravel.OfficialTravelDetail; // 價格資訊從這裡來

                    switch (cartItemDto.OptionType.ToUpper())
                    {
                        case "成人": case "ADULT": unitPrice = priceDetail.AdultPrice ?? 0; break;
                        case "兒童加床": unitPrice = priceDetail.AdultPrice ?? 0; optionSpecificDescription = "兒童加床"; break; // 假設加床價格與成人相同
                        case "兒童佔床": unitPrice = priceDetail.ChildPrice ?? 0; optionSpecificDescription = "兒童佔床"; break;
                        case "兒童不佔床": unitPrice = priceDetail.BabyPrice ?? 0; optionSpecificDescription = "兒童不佔床"; break; // 假設不佔床同嬰兒價
                        case "兒童": unitPrice = priceDetail.ChildPrice ?? 0; break;
                        case "嬰兒": case "BABY": unitPrice = priceDetail.BabyPrice ?? 0; break;
                        default:
                            _logger.LogWarning($"團體行程 '{productName}' (GT{cartItemDto.ProductId}) 的選項類型 '{cartItemDto.OptionType}' 無法識別或未定價。");
                            return BadRequest($"商品 '{productName}' 的選項類型 '{cartItemDto.OptionType}' 無法識別或未提供價格。");
                    }

                    if (unitPrice <= 0 && cartItemDto.Quantity > 0)
                    {
                        _logger.LogWarning($"團體行程 '{productName}' 選項 '{cartItemDto.OptionType}' 的價格異常 ({unitPrice})。");
                        return BadRequest($"商品 '{productName}' 選項 '{cartItemDto.OptionType}' 的價格資料異常。");
                    }
                    itemSpecificStartDate = groupTravel.DepartureDate;
                    itemSpecificEndDate = groupTravel.ReturnDate;
                }
                else if (itemCategory == ProductCategory.CustomTravel)
                {
                    var customTravel = await _context.CustomTravels
                                             .FirstOrDefaultAsync(ct => ct.CustomTravelId == cartItemDto.ProductId && ct.Status == CustomTravelStatus.Pending); // 假設是 Pending 狀態
                    if (customTravel == null)
                    {
                        return BadRequest($"客製化行程產品 (ID: CT{cartItemDto.ProductId}) 無效或未發佈。");
                    }

                    unitPrice = customTravel.TotalAmount; // CustomTravel 的價格是其 TotalAmount
                    productName = customTravel.Note ?? $"客製化行程 {customTravel.CustomTravelId}"; // 使用 Note 作為名稱，或預設名稱
                    //optionSpecificDescription = cartItemDto.OptionType; // 例如 "全包方案" 或前端傳來的選項描述

                    if (unitPrice <= 0 && cartItemDto.Quantity > 0)
                    {
                        _logger.LogWarning($"團體行程 '{productName}' 選項 '{cartItemDto.OptionType}' 的價格異常 ({unitPrice})。");
                        return BadRequest($"商品 '{productName}' 選項 '{cartItemDto.OptionType}' 的價格資料異常。");
                    }
                }
                else
                {
                    return BadRequest($"系統內部錯誤：無法處理的商品類型 {itemCategory}。");
                }

                var orderDetail = new OrderDetail
                {
                    Order = order,
                    Category = itemCategory,                     
                    ItemId = cartItemDto.ProductId, // ItemId (GroupTravelId 或 CustomTravelId)
                    Description = $"{productName} - {optionSpecificDescription}",
                    Quantity = cartItemDto.Quantity,
                    Price = unitPrice,
                    TotalAmount = cartItemDto.Quantity * unitPrice,
                    OrderParticipants = new List<OrderParticipant>(),
                    StartDate = itemSpecificStartDate,
                    EndDate = itemSpecificEndDate
                };
                order.OrderDetails.Add(orderDetail);
                calculatedServerTotalAmount += orderDetail.TotalAmount;
            }

            // 使用伺服器計算的總金額，並可選擇性地與前端傳來的總金額進行比較
            order.TotalAmount = calculatedServerTotalAmount;
            //if (Math.Abs(calculatedServerTotalAmount - orderCreateDto.TotalAmount) > 0.01m) // 允許0.01的誤差
            //{
            //    _logger.LogWarning("訂單總金額不一致。會員ID {MemberId}。伺服器計算: {CalculatedTotal}, 前端提供: {DtoTotal}.",
            //    member.MemberId, calculatedServerTotalAmount, orderCreateDto.TotalAmount);
            //}

            // 4. 處理旅客列表 (OrderParticipants)
            foreach (var detail in order.OrderDetails) // 遍歷剛剛根據 CartItems 生成的 OrderDetail 實體
            {
                // 根據這個 OrderDetail 的 Quantity，分配相應數量的旅客
                for (int i = 0; i < detail.Quantity; i++)
                {
                    if (orderCreateDto.Participants != null && participantDtoGlobalIndex < orderCreateDto.Participants.Count)
                    {
                        var participantDto = orderCreateDto.Participants[participantDtoGlobalIndex++]; // 按順序取一個旅客 DTO

                        // --- 你提供的程式碼片段從這裡開始 ---
                        if (participantDto.DocumentType == DocumentType.PASSPORT && string.IsNullOrEmpty(participantDto.DocumentNumber))
                        {
                            ModelState.AddModelError($"Participants[{participantDtoGlobalIndex - 1}].DocumentNumber", "選擇護照作為證件類型時，護照號碼為必填。");
                        }
                        if (string.IsNullOrEmpty(participantDto.IdNumber) && string.IsNullOrEmpty(participantDto.DocumentNumber))
                        {
                            ModelState.AddModelError($"Participants[{participantDtoGlobalIndex - 1}].IdOrDocumentNumber", "旅客身分證號或證件號碼至少需填寫一項。");
                        }
                        if (!ModelState.IsValid)
                        {
                            _logger.LogWarning("CreateOrder Participant Custom Validation Failed: {@ModelStateErrors}", ModelState);
                            return BadRequest(ModelState);
                        }

                        var participant = new OrderParticipant
                        {
                            Order = order,           // <--- 關聯到主訂單
                            OrderDetail = detail,    // <--- 關聯到當前的 OrderDetail (來自外層迴圈的 detail)
                            Name = participantDto.Name,
                            BirthDate = participantDto.BirthDate,
                            IdNumber = participantDto.IdNumber,
                            Gender = participantDto.Gender, // 來自 DTO，其類型應與 OrderParticipant.Gender (枚舉) 兼容
                            //Phone = participantDto.Phone,
                            //Email = participantDto.Email,
                            DocumentType = participantDto.DocumentType, // 來自 DTO，其類型應與 OrderParticipant.DocumentType (枚舉) 兼容
                            DocumentNumber = participantDto.DocumentNumber,
                            PassportSurname = participantDto.PassportSurname,
                            PassportGivenName = participantDto.PassportGivenName,
                            PassportExpireDate = participantDto.PassportExpireDate,
                            Nationality = participantDto.Nationality,
                            Note = participantDto.Note,

                            FavoriteTravelerId = participantDto.FavoriteTravelerId
                        };
                        detail.OrderParticipants.Add(participant); // 將 participant 加入到其所屬的 OrderDetail 的集合中
                        //order.OrderParticipants.Add(participant); // 或者統一加入到 Order 的總旅客集合中，EF Core會處理關係


                        // 步驟 5: 嘗試從常用旅客讀取資料
                        if (participantDto.FavoriteTravelerId.HasValue && participantDto.FavoriteTravelerId.Value > 0)
                        {
                            // 嘗試從常用旅客讀取資料
                            var favoriteTraveler = await _context.MemberFavoriteTravelers
                                .AsNoTracking()
                                .FirstOrDefaultAsync(ft => ft.FavoriteTravelerId == participantDto.FavoriteTravelerId.Value &&
                                                           ft.MemberId == member.MemberId && // 確保是該會員的常用旅客
                                                           ft.Status == FavoriteStatus.Active); // 確保常用旅客是有效的

                            if (favoriteTraveler != null)
                            {
                                // 使用常用旅客資料預填 OrderParticipant
                                participant.Name = favoriteTraveler.Name;
                                participant.BirthDate = favoriteTraveler.BirthDate ?? default; // 如果常用旅客生日可為null，提供預設值
                                participant.IdNumber = favoriteTraveler.IdNumber;
                                participant.Gender = favoriteTraveler.Gender ?? default; // 同上
                                //participant.Phone = favoriteTraveler.Phone;
                                //participant.Email = favoriteTraveler.Email;
                                participant.DocumentType = favoriteTraveler.DocumentType ?? default; // 同上
                                participant.DocumentNumber = favoriteTraveler.DocumentNumber;
                                participant.PassportSurname = favoriteTraveler.PassportSurname;
                                participant.PassportGivenName = favoriteTraveler.PassportGivenName;
                                participant.PassportExpireDate = favoriteTraveler.PassportExpireDate;
                                participant.Nationality = favoriteTraveler.Nationality;
                                participant.Note = string.IsNullOrEmpty(participantDto.Note) ? favoriteTraveler.Note : participantDto.Note;
                            }
                            else
                            {
                                _logger.LogWarning($"嘗試使用常用旅客ID: {participantDto.FavoriteTravelerId.Value} 但找不到或該旅客已停用。將使用前端提供的旅客資料。");
                            }
                        }
                        if (participantDto.FavoriteTravelerId.HasValue && participantDto.FavoriteTravelerId.Value > 0)
                        {
                            var favoriteTraveler = await _context.MemberFavoriteTravelers
                                .FirstOrDefaultAsync(ft => ft.FavoriteTravelerId == participantDto.FavoriteTravelerId.Value &&
                                                          ft.MemberId == member.MemberId &&
                                                          ft.Status == FavoriteStatus.Active);
                            if (favoriteTraveler != null)
                            {
                                // 更新 participant 的屬性
                                participant.Name = favoriteTraveler.Name;
                                participant.BirthDate = favoriteTraveler.BirthDate ?? default;
                                // ... 更新所有相關欄位 ...
                                participant.Note = favoriteTraveler.Note; // 或 participantDto.Note
                            }
                            else
                            {
                                return BadRequest($"找不到常用旅客 ID: {participantDto.FavoriteTravelerId.Value} 或該旅客已停用。");
                            }
                        }
                    }
                    else
                    {
                        // 旅客 DTO 數量不足以填滿所有 OrderDetail 的 Quantity
                        _logger.LogWarning("提供的旅客資料數量不足以滿足所有訂單明細項的需求。");
                        ModelState.AddModelError("Participants", "旅客資料數量與訂單商品所需數量不符。");
                        return BadRequest(ModelState); // 或者其他處理方式
                    }
                
                }
            }

            // 在嘗試儲存到資料庫之前，再次檢查 ModelState，包括剛才添加的自定義錯誤
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
                    await _context.SaveChangesAsync(); // 第一次儲存以獲取 OrderId

                    // 處理常用旅客的更新或新增
                    if (orderCreateDto.TravelerProfileActions != null && orderCreateDto.TravelerProfileActions.Any())
                    {
                        foreach (var profileAction in orderCreateDto.TravelerProfileActions)
                        {
                            if (profileAction.FavoriteTravelerIdToUpdate.HasValue && profileAction.FavoriteTravelerIdToUpdate.Value > 0)
                            {
                                // 更新現有的常用旅客
                                var existingFavTraveler = await _context.MemberFavoriteTravelers
                                    .FirstOrDefaultAsync(ft => ft.FavoriteTravelerId == profileAction.FavoriteTravelerIdToUpdate.Value &&
                                                                ft.MemberId == member.MemberId); // 安全性檢查：確保是該會員的

                                if (existingFavTraveler != null)
                                {
                                    _logger.LogInformation("準備更新常用旅客資料，常用旅客ID: {FavoriteTravelerId}, 會員ID: {MemberId}",
                                        existingFavTraveler.FavoriteTravelerId, member.MemberId);

                                    // 更新欄位 (假設 MemberFavoriteTraveler 有這些欄位)
                                    existingFavTraveler.Name = profileAction.Name;
                                    existingFavTraveler.BirthDate = profileAction.BirthDate;
                                    existingFavTraveler.IdNumber = profileAction.IdNumber;
                                    existingFavTraveler.Gender = profileAction.Gender;
                                    // existingFavTraveler.Phone = profileAction.Phone; // 如果常用旅客有電話
                                    // existingFavTraveler.Email = profileAction.Email; // 如果常用旅客有Email
                                    existingFavTraveler.DocumentType = profileAction.DocumentType;
                                    existingFavTraveler.DocumentNumber = profileAction.DocumentNumber;
                                    existingFavTraveler.PassportSurname = profileAction.PassportSurname;
                                    existingFavTraveler.PassportGivenName = profileAction.PassportGivenName;
                                    existingFavTraveler.PassportExpireDate = profileAction.PassportExpireDate;
                                    existingFavTraveler.Nationality = profileAction.Nationality;
                                    // existingFavTraveler.Note = profileAction.Note; // 如果常用旅客有備註
                                    existingFavTraveler.UpdatedAt = DateTime.Now;
                                    // Status 通常保持 Active，除非有特別邏輯

                                    _context.MemberFavoriteTravelers.Update(existingFavTraveler);
                                }
                                else
                                {
                                    _logger.LogWarning("嘗試更新不存在或不屬於該會員的常用旅客資料，常用旅客ID: {FavoriteTravelerIdToUpdate}, 會員ID: {MemberId}",
                                        profileAction.FavoriteTravelerIdToUpdate, member.MemberId);
                                    // 可選擇是否要因此中斷交易或僅記錄
                                }
                            }
                            else
                            {
                                // 新增常用旅客
                                _logger.LogInformation("準備新增常用旅客資料，會員ID: {MemberId}, 旅客姓名: {TravelerName}",
                                    member.MemberId, profileAction.Name);

                                var newFavTraveler = new MemberFavoriteTraveler
                                {
                                    MemberId = member.MemberId,
                                    Name = profileAction.Name,
                                    BirthDate = profileAction.BirthDate,
                                    IdNumber = profileAction.IdNumber,
                                    Gender = profileAction.Gender,
                                    // Phone = profileAction.Phone,
                                    // Email = profileAction.Email,
                                    DocumentType = profileAction.DocumentType,
                                    DocumentNumber = profileAction.DocumentNumber,
                                    PassportSurname = profileAction.PassportSurname,
                                    PassportGivenName = profileAction.PassportGivenName,
                                    PassportExpireDate = profileAction.PassportExpireDate,
                                    Nationality = profileAction.Nationality,
                                    // Note = profileAction.Note,
                                    Status = FavoriteStatus.Active, // 設定預設狀態
                                    CreatedAt = DateTime.Now,    
                                    UpdatedAt = DateTime.Now
                                };
                                _context.MemberFavoriteTravelers.Add(newFavTraveler);
                            }
                        }
                        await _context.SaveChangesAsync(); // 儲存常用旅客的變更
                    }

                    await transaction.CommitAsync();
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "建立訂單時資料庫更新失敗。InnerException: {InnerMessage}", ex.InnerException?.Message);
                    return StatusCode(StatusCodes.Status500InternalServerError, "建立訂單失敗，請稍後再試。");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "建立訂單時發生未預期錯誤。");
                    return StatusCode(StatusCodes.Status500InternalServerError, "建立訂單時發生錯誤。");
                }
            }

            // 準備回應給前端的資料
            // 回傳訂單ID，以及支付所需的相關資訊 (這部分取決於您與支付閘道的整合方式)
            var orderSummary = new OrderSummaryDto
            {
                OrderId = order.OrderId,
                MerchantTradeNo = order.MerchantTradeNo, // 重要，用於後續付款
                OrderStatus = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                ExpiresAt = order.ExpiresAt // 重要，用於前端顯示倒數
            };
            _logger.LogInformation("初步訂單建立成功，回傳 orderSummary: {@OrderSummary}", orderSummary);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId, memberId = order.MemberId }, orderSummary);
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

        [HttpPut("{orderId}")] // 或者 [HttpPost("{orderId}/update")]
        public async Task<ActionResult<OrderSummaryDto>> UpdateOrderAsync(int orderId, [FromBody] OrderUpdateDto orderUpdateDto)
        {
            _logger.LogInformation("接收到訂單更新請求，OrderID: {OrderId}, DTO: {@OrderUpdateDto}", orderId, orderUpdateDto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("訂單更新模型驗證失敗: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            //var currentMemberId = GetCurrentUserId(); // 獲取當前登入用戶的 MemberId
            //if (currentMemberId == null)
            //{
            //    _logger.LogWarning("訂單更新：用戶未認證或無法獲取 MemberId。");
            //    return Unauthorized(new { message = "用戶未認證。" });
            //}

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var order = await _context.Orders
                        .Include(o => o.OrderDetails)
                            .ThenInclude(od => od.OrderParticipants) // 載入 OrderDetail 下的 Participants
                        .FirstOrDefaultAsync(o => o.OrderId == orderId);
                        //.FirstOrDefaultAsync(o => o.OrderId == orderId && o.MemberId == currentMemberId.Value);

                    if (order == null)
                    {
                        _logger.LogWarning("訂單更新：找不到訂單 ID {OrderId} 或會員 {CurrentMemberId} 無權修改此訂單。", orderId);
                        //_logger.LogWarning("訂單更新：找不到訂單 ID {OrderId} 或會員 {CurrentMemberId} 無權修改此訂單。", orderId, currentMemberId.Value);
                        return NotFound(new { message = $"找不到訂單 {orderId} 或您無權修改此訂單。" });
                    }

                    if (order.Status != OrderStatus.Awaiting) // 假設只有 Awaiting 狀態的訂單可以修改
                    {
                        _logger.LogWarning("訂單更新：訂單 {OrderId} 狀態為 {Status}，無法修改。", orderId, order.Status);
                        return BadRequest(new { message = $"訂單目前狀態為 '{order.Status}'，無法修改。" });
                    }

                    // 1. 更新 Order 的基本資料
                    order.OrdererName = orderUpdateDto.OrdererInfo.Name;
                    order.OrdererPhone = NormalizePhoneNumber(orderUpdateDto.OrdererInfo.MobilePhone); // 假設你有 NormalizePhoneNumber
                    order.OrdererEmail = orderUpdateDto.OrdererInfo.Email;
                    order.OrdererNationality = orderUpdateDto.OrdererInfo.Nationality;
                    order.OrdererDocumentNumber = orderUpdateDto.OrdererInfo.DocumentNumber;
                    order.OrdererDocumentType = orderUpdateDto.OrdererInfo.DocumentType; // 假設 DTO 的 DocumentType 是 string，且 OrdererDocumentType 是枚舉
                    order.Note = orderUpdateDto.OrderNotes;
                    order.UpdatedAt = DateTime.Now;
                    // 通常 MerchantTradeNo 在訂單建立後不應更改
                    order.ExpiresAt = DateTime.Now.AddMinutes(3); // 根據你的業務邏輯調整

                    // 2. 刪除舊的 OrderParticipants 和 OrderDetails
                    if (order.OrderDetails.Any())
                    {
                        // 先刪除依賴 OrderDetail 的 OrderParticipant
                        var participantsToRemove = order.OrderDetails.SelectMany(od => od.OrderParticipants).ToList();
                        if (participantsToRemove.Any())
                        {
                            _context.OrderParticipants.RemoveRange(participantsToRemove);
                        }
                        _context.OrderDetails.RemoveRange(order.OrderDetails);
                        // 清空記憶體中的集合是好習慣，但EF Core追蹤實體狀態，RemoveRange通常足夠
                        // order.OrderDetails.Clear(); 
                        // order.OrderParticipants.Clear(); // 也清空直接掛在order下的 (如果之前有這樣加)
                    }
                    // **重要**: 如果你先 SaveChanges() 執行刪除，後續添加時ID不會衝突。
                    // 但為了在同一個交易內，我們嘗試讓 EF Core 處理。
                    // 如果遇到問題，可以考慮在這裡 SaveChanges 一次。

                    // 2c. 根據 orderUpdateDto.CartItems 重新創建 OrderDetails 和其關聯的 OrderParticipants
                    decimal calculatedServerTotalAmount = 0;
                    int participantDtoGlobalIndex = 0;

                    if (orderUpdateDto.CartItems == null || !orderUpdateDto.CartItems.Any())
                    {
                        ModelState.AddModelError("CartItems", "更新訂單時，購物車中沒有商品。");
                        return BadRequest(ModelState);
                    }

                    var newOrderDetails = new List<OrderDetail>(); // 暫存新的 OrderDetail

                    foreach (var cartItemDto in orderUpdateDto.CartItems)
                    {
                        // --- 這部分邏輯與 InitiateOrderAsync 中的創建 OrderDetail 和 Participant 邏輯幾乎一致 ---
                        decimal unitPrice = 0;
                        string productName = ""; // 預設或從產品查找
                        string optionSpecificDescription = cartItemDto.OptionType;
                        ProductCategory itemCategory;

                        // 新增：用於儲存從產品獲取的日期
                        DateTime? itemSpecificStartDate = null;
                        DateTime? itemSpecificEndDate = null;

                        if (!Enum.TryParse<ProductCategory>(cartItemDto.ProductType, true, out var parsedCategory))
                        {
                            _logger.LogWarning($"訂單更新 {orderId}：無效的 ProductType: {cartItemDto.ProductType}。");
                            return BadRequest($"不支援的商品類型: {cartItemDto.ProductType}。");
                        }
                        itemCategory = parsedCategory;

                        // (複製 InitiateOrderAsync 中根據 itemCategory 獲取 productName 和 unitPrice 的邏輯)
                        if (itemCategory == ProductCategory.GroupTravel)
                        {
                            var groupTravel = await _context.GroupTravels
                                .Include(gt => gt.OfficialTravelDetail).ThenInclude(otd => otd.OfficialTravel)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(gt => gt.GroupTravelId == cartItemDto.ProductId && gt.GroupStatus == "可報名");

                            if (groupTravel == null || groupTravel.OfficialTravelDetail?.OfficialTravel == null || groupTravel.OfficialTravelDetail.State != DetailState.Locked)
                            { return BadRequest($"團體行程 (ID: GT{cartItemDto.ProductId}) 資訊不完整或未鎖定價格。"); }
                            productName = groupTravel.OfficialTravelDetail.OfficialTravel.Title;
                            var priceDetail = groupTravel.OfficialTravelDetail;
                            switch (cartItemDto.OptionType.ToUpper()) // 假設 OptionType 是 "成人", "兒童" 等
                            {
                                case "成人": case "ADULT": unitPrice = priceDetail.AdultPrice ?? 0; break;
                                case "兒童加床": unitPrice = priceDetail.AdultPrice ?? 0; optionSpecificDescription = "兒童加床"; break; // 假設加床價格與成人相同
                                case "兒童佔床": unitPrice = priceDetail.ChildPrice ?? 0; optionSpecificDescription = "兒童佔床"; break;
                                case "兒童不佔床": unitPrice = priceDetail.BabyPrice ?? 0; optionSpecificDescription = "兒童不佔床"; break; // 假設不佔床同嬰兒價
                                case "兒童": case "CHILD": unitPrice = priceDetail.ChildPrice ?? 0; break; // 調整為 CHILD
                                case "嬰兒": case "BABY": unitPrice = priceDetail.BabyPrice ?? 0; break;
                                default: return BadRequest($"商品 '{productName}' 的選項類型 '{cartItemDto.OptionType}' 無法識別。");
                            }
                            if (unitPrice <= 0 && cartItemDto.Quantity > 0) return BadRequest($"商品 '{productName}' 選項 '{cartItemDto.OptionType}' 的價格資料異常。");
                            
                            // 從 GroupTravel 獲取日期
                            itemSpecificStartDate = groupTravel.DepartureDate;
                            itemSpecificEndDate = groupTravel.ReturnDate;
                        }
                        else if (itemCategory == ProductCategory.CustomTravel)
                        {
                            var customTravel = await _context.CustomTravels.FirstOrDefaultAsync(ct => ct.CustomTravelId == cartItemDto.ProductId && ct.Status == CustomTravelStatus.Pending);
                            if (customTravel == null) { return BadRequest($"客製化行程產品 (ID: CT{cartItemDto.ProductId}) 無效。"); }
                            unitPrice = customTravel.TotalAmount;
                            productName = customTravel.Note ?? $"客製化行程 {customTravel.CustomTravelId}";
                            if (unitPrice <= 0 && cartItemDto.Quantity > 0) return BadRequest($"商品 '{productName}' 價格資料異常。");
                        }
                        else { return BadRequest($"未知的商品類型 {itemCategory}。"); }


                        var orderDetail = new OrderDetail
                        {
                            Order = order, // 自動設定 OrderId
                            Category = itemCategory,
                            ItemId = cartItemDto.ProductId,
                            Description = $"{productName} - {optionSpecificDescription}",
                            Quantity = cartItemDto.Quantity,
                            Price = unitPrice,
                            TotalAmount = cartItemDto.Quantity * unitPrice,
                            OrderParticipants = new List<OrderParticipant>(), // 初始化
                            StartDate = itemSpecificStartDate,
                            EndDate = itemSpecificEndDate
                        };
                        newOrderDetails.Add(orderDetail);
                        calculatedServerTotalAmount += orderDetail.TotalAmount;

                        for (int i = 0; i < orderDetail.Quantity; i++)
                        {
                            if (orderUpdateDto.Participants != null && participantDtoGlobalIndex < orderUpdateDto.Participants.Count)
                            {
                                var participantDto = orderUpdateDto.Participants[participantDtoGlobalIndex++];
                                if (participantDto.DocumentType == DocumentType.PASSPORT && string.IsNullOrEmpty(participantDto.DocumentNumber))
                                { 
                                    ModelState.AddModelError($"Participants[{participantDtoGlobalIndex - 1}].DocumentNumber", "選擇護照作為證件類型時，護照號碼為必填。"); 
                                }
                                if (string.IsNullOrEmpty(participantDto.IdNumber) && string.IsNullOrEmpty(participantDto.DocumentNumber))
                                {
                                    ModelState.AddModelError($"Participants[{participantDtoGlobalIndex - 1}].IdOrDocumentNumber", "旅客身分證號或證件號碼至少需填寫一項。"); 
                                }
                                if (!ModelState.IsValid) 
                                {
                                    return BadRequest(ModelState); 
                                }
                                var participant = new OrderParticipant
                                {
                                    Order = order,
                                    OrderDetail = orderDetail, // **關聯到新的 OrderDetail**
                                    Name = participantDto.Name,
                                    BirthDate = participantDto.BirthDate,
                                    IdNumber = participantDto.IdNumber,
                                    Gender = participantDto.Gender, // DTO應為枚舉或可轉換的字串/數字
                                    Phone = participantDto.Phone,
                                    Email = participantDto.Email,
                                    DocumentType = participantDto.DocumentType, // DTO應為枚舉或可轉換的字串/數字
                                    DocumentNumber = participantDto.DocumentNumber,
                                    PassportSurname = participantDto.PassportSurname,
                                    PassportGivenName = participantDto.PassportGivenName,
                                    PassportExpireDate = participantDto.PassportExpireDate,
                                    Nationality = participantDto.Nationality,
                                    Note = participantDto.Note,
                                    FavoriteTravelerId = participantDto.FavoriteTravelerId
                                };
                                if (participantDto.FavoriteTravelerId.HasValue && participantDto.FavoriteTravelerId.Value > 0)
                                {
                                    // 嘗試從常用旅客讀取資料
                                    var favoriteTraveler = await _context.MemberFavoriteTravelers
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(ft => ft.FavoriteTravelerId == participantDto.FavoriteTravelerId.Value &&
                                                                   ft.MemberId == order.MemberId &&
                                                                   ft.Status == FavoriteStatus.Active); // 確保常用旅客是有效的

                                    if (favoriteTraveler != null)
                                    {
                                        // 使用常用旅客資料預填 OrderParticipant
                                        participant.Name = favoriteTraveler.Name;
                                        participant.BirthDate = favoriteTraveler.BirthDate ?? default; // 如果常用旅客生日可為null，提供預設值
                                        participant.IdNumber = favoriteTraveler.IdNumber;
                                        participant.Gender = favoriteTraveler.Gender ?? default; // 同上
                                                                                                 //participant.Phone = favoriteTraveler.Phone;
                                                                                                 //participant.Email = favoriteTraveler.Email;
                                        participant.DocumentType = favoriteTraveler.DocumentType ?? default; // 同上
                                        participant.DocumentNumber = favoriteTraveler.DocumentNumber;
                                        participant.PassportSurname = favoriteTraveler.PassportSurname;
                                        participant.PassportGivenName = favoriteTraveler.PassportGivenName;
                                        participant.PassportExpireDate = favoriteTraveler.PassportExpireDate;
                                        participant.Nationality = favoriteTraveler.Nationality;
                                        participant.Note = string.IsNullOrEmpty(participantDto.Note) ? favoriteTraveler.Note : participantDto.Note;
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"更新訂單時，常用旅客ID: {participantDto.FavoriteTravelerId.Value} 找不到、不屬於會員 {order.MemberId} 或已停用。");
                                    }
                                }
                                orderDetail.OrderParticipants.Add(participant);
                            }
                            else
                            {
                                _logger.LogWarning("更新訂單 OrderID {OrderId} 時，商品 {ProductName} 提供的旅客資料數量不足。", orderId, productName);
                                ModelState.AddModelError("Participants", $"商品 '{productName} - {optionSpecificDescription}' 的旅客資料不足。");
                                return BadRequest(ModelState);
                            }
                        }
                    }
                    order.OrderDetails = newOrderDetails;
                    order.TotalAmount = calculatedServerTotalAmount;
                    if (Math.Abs(calculatedServerTotalAmount - orderUpdateDto.TotalAmount) > 0.01m)
                    {
                        _logger.LogWarning("訂單 {OrderId} 更新時總金額不一致。伺服器: {Calculated}, 前端: {Provided}", orderId, calculatedServerTotalAmount, orderUpdateDto.TotalAmount);
                        // 根據業務邏輯決定是否要因此返回錯誤
                    }

                    // 3. 處理 TravelerProfileActions (如果需要，邏輯與 InitiateOrderAsync 類似)
                    if (orderUpdateDto.TravelerProfileActions != null && orderUpdateDto.TravelerProfileActions.Any())
                    {
                        // ... (你的更新/新增常用旅客邏輯) ...
                    }

                    if (!ModelState.IsValid)
                    {
                        _logger.LogWarning("UpdateOrder 最終 ModelState 無效: {@ModelStateErrors}", ModelState.Values.SelectMany(v => v.Errors));
                        return BadRequest(ModelState);
                    }

                    await _context.SaveChangesAsync(); // 一次性儲存所有變更
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