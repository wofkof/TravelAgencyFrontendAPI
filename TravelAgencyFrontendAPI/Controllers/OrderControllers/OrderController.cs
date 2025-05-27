
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyFrontendAPI.DTOs.OrderDTOs;
using Microsoft.Extensions.Logging; // 加入 ILogger
using System; // 加入 System
using System.Collections.Generic; // 加入 List
using System.Linq; // 加入 Linq
using System.Threading.Tasks; // 加入 Task
using System.ComponentModel.DataAnnotations; // 如果 DTO 使用 DataAnnotations

namespace TravelAgencyFrontendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize] // 加上這個，確保只有登入會員才能下單
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(AppDbContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/Order
        // 建立新的訂單 (初始狀態，待付款)
        [HttpPost]
        public async Task<ActionResult<OrderSummaryDto>> CreateOrder([FromBody] OrderCreateDto orderCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var memberIdFromDto = orderCreateDto.MemberId;
            var member = await _context.Members.FirstOrDefaultAsync(m => m.MemberId == memberIdFromDto);
            if (member == null)
            {
                _logger.LogWarning("CreateOrder 找不到會員，會員 ID (來自前端DTO): {MemberId}", memberIdFromDto);
                // 返回明確的錯誤，告知前端提供的 MemberId 找不到對應的會員
                ModelState.AddModelError(nameof(orderCreateDto.MemberId), $"提供的會員 ID {memberIdFromDto} 不存在。");
                return BadRequest(ModelState);
            }

            // 初始化 Order 物件
            var order = new Order
            {
                MemberId = member.MemberId,

                OrdererName = orderCreateDto.OrdererInfo.Name, // 訂購人姓名
                OrdererPhone = NormalizePhoneNumber(orderCreateDto.OrdererInfo.MobilePhone), // 訂購人電話
                OrdererEmail = orderCreateDto.OrdererInfo.Email, // 訂購人Email

                PaymentMethod = orderCreateDto.SelectedPaymentMethod, // 使用者選擇的付款方式
                Status = OrderStatus.Awaiting, // 初始狀態設為Awaiting(待付款)
                CreatedAt = DateTime.UtcNow, // 使用 UTC 時間
                Note = orderCreateDto.OrderNotes,
                // 處理發票請求資訊
                InvoiceOption = orderCreateDto.InvoiceRequestInfo.InvoiceOption,
                InvoiceDeliveryEmail = orderCreateDto.InvoiceRequestInfo.InvoiceDeliveryEmail,
                InvoiceUniformNumber = orderCreateDto.InvoiceRequestInfo.InvoiceUniformNumber,
                InvoiceTitle = orderCreateDto.InvoiceRequestInfo.InvoiceTitle,
                InvoiceAddBillingAddr = orderCreateDto.InvoiceRequestInfo.InvoiceAddBillingAddr,
                InvoiceBillingAddress = orderCreateDto.InvoiceRequestInfo.InvoiceBillingAddress,
                OrderDetails = new List<OrderDetail>(), // 初始化訂單明細集合
                OrderParticipants = new List<OrderParticipant>() // 初始化旅客集合
            };

            decimal calculatedServerTotalAmount = 0;
            if (orderCreateDto.CartItems == null || !orderCreateDto.CartItems.Any())
            {
                ModelState.AddModelError("CartItems", "購物車中沒有商品。");
                return BadRequest(ModelState);
            }
            // --- 核心修改：根據前端傳來的 CartItems 生成 OrderDetails ---
            foreach (var cartItemDto in orderCreateDto.CartItems)
            {
                decimal unitPrice = 0;
                string productName = "";
                string optionSpecificDescription = cartItemDto.OptionType; // 選項的描述
                ProductCategory itemCategory; // 用來儲存轉換後的 ProductCategory

                if (Enum.TryParse<ProductCategory>(cartItemDto.ProductType, true, out var parsedCategory)) // true 表示忽略大小寫
                {
                    itemCategory = parsedCategory;
                }
                else
                {
                    _logger.LogWarning($"無效的 ProductType: {cartItemDto.ProductType}。");
                    return BadRequest($"不支援的商品類型: {cartItemDto.ProductType}。");
                }
                if (itemCategory == ProductCategory.GroupTravel)
                {
                    var groupTravel = await _context.GroupTravels
                                                .Include(gt => gt.OfficialTravelDetail)
                                                    .ThenInclude(otd => otd.OfficialTravel)
                                                .FirstOrDefaultAsync(gt => gt.GroupTravelId == cartItemDto.ProductId && gt.GroupStatus == "開團");

                    if (groupTravel == null)
                    {
                        return BadRequest($"團體行程 (ID: GT{cartItemDto.ProductId}) 無效、未開放報名，或找不到對應的行程資料。");
                    }
                    if (groupTravel.OfficialTravelDetail == null)
                    {
                        return BadRequest($"團體行程 (ID: GT{cartItemDto.ProductId}) 缺少必要的價格明細設定 (OfficialTravelDetail)。");
                    }
                    if (groupTravel.OfficialTravelDetail.OfficialTravel == null)
                    {
                        return BadRequest($"團體行程 (ID: GT{cartItemDto.ProductId}) 關聯的官方行程主檔資料遺失。");
                    }
                    if (groupTravel.OfficialTravelDetail.State != DetailState.Locked) // 假設價格必須是鎖定狀態
                    {
                        return BadRequest($"團體行程 (ID: GT{cartItemDto.ProductId}) 的價格資訊目前未鎖定，無法下單。");
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
                    optionSpecificDescription = cartItemDto.OptionType; // 例如 "全包方案" 或前端傳來的選項描述

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
                    //OrderId = order.OrderId,
                    Category = itemCategory,                     // << 賦值 ProductCategory
                    ItemId = cartItemDto.ProductId,              // << 賦值 ItemId (GroupTravelId 或 CustomTravelId)
                    Description = $"{productName} - {optionSpecificDescription}",
                    Quantity = cartItemDto.Quantity,
                    Price = unitPrice,
                    TotalAmount = cartItemDto.Quantity * unitPrice,
                };
                order.OrderDetails.Add(orderDetail);
                calculatedServerTotalAmount += orderDetail.TotalAmount;
            }

            // 使用伺服器計算的總金額，並可選擇性地與前端傳來的總金額進行比較
            order.TotalAmount = calculatedServerTotalAmount;
            if (Math.Abs(calculatedServerTotalAmount - orderCreateDto.TotalAmount) > 0.01m) // 允許0.01的誤差
            {
                _logger.LogWarning("訂單總金額不一致。會員ID {MemberId}。伺服器計算: {CalculatedTotal}, 前端提供: {DtoTotal}.",
                member.MemberId, calculatedServerTotalAmount, orderCreateDto.TotalAmount);
            }

            // 4. 處理旅客列表 (OrderParticipants)
            if (orderCreateDto.Participants != null)
            {
                foreach (var participantDto in orderCreateDto.Participants)
                {
                    if (participantDto.DocumentType == DocumentType.PASSPORT && string.IsNullOrEmpty(participantDto.DocumentNumber))
                    {
                        // 添加一個 Model 錯誤或直接返回 BadRequest
                        ModelState.AddModelError($"Participants[{order.OrderParticipants.Count}].DocumentNumber", "選擇護照作為證件類型時，護照號碼為必填。");
                    }

                    // 檢查必填欄位是否有值 (雖然 DTO 已有 [Required]，但這裡可以再次確認或記錄)
                    if (string.IsNullOrEmpty(participantDto.Name)) { /* ... 錯誤處理 ... */ }
                    // ... 其他必填欄位檢查 ...

                    // 或者其他組合規則，例如：IdNumber 或 DocumentNumber 至少要有一個非空？
                    if (string.IsNullOrEmpty(participantDto.IdNumber) && string.IsNullOrEmpty(participantDto.DocumentNumber))
                    {
                        ModelState.AddModelError($"Participants[{order.OrderParticipants.Count}].IdOrDocumentNumber", "旅客身分證號或證件號碼至少需填寫一項。");
                    }

                    // 如果有自定義驗證錯誤，建議在這裡檢查並提前返回
                    if (!ModelState.IsValid)
                    {
                        // 記錄錯誤並返回
                        _logger.LogWarning("CreateOrder Participant Custom Validation Failed: {@ModelStateErrors}", ModelState);
                        return BadRequest(ModelState);
                    }

                    var participant = new OrderParticipant
                    {
                        Name = participantDto.Name,
                        BirthDate = participantDto.BirthDate,
                        IdNumber = participantDto.IdNumber,
                        Gender = participantDto.Gender,

                        Phone = participantDto.Phone,
                        Email = participantDto.Email,

                        DocumentType = participantDto.DocumentType,
                        DocumentNumber = participantDto.DocumentNumber,
                        PassportSurname = participantDto.PassportSurname,
                        PassportGivenName = participantDto.PassportGivenName,
                        PassportExpireDate = participantDto.PassportExpireDate,
                        Nationality = participantDto.Nationality,
                        Note = participantDto.Note
                    };
                    order.OrderParticipants.Add(participant);
                    // 步驟 5: 嘗試從常用旅客讀取資料
                    if (participantDto.FavoriteTravelerId.HasValue && participantDto.FavoriteTravelerId.Value > 0)
                    {
                        // 嘗試從常用旅客讀取資料
                        var favoriteTraveler = await _context.MemberFavoriteTravelers
                            .FirstOrDefaultAsync(ft => ft.FavoriteTravelerId == participantDto.FavoriteTravelerId.Value &&
                                                       ft.MemberId == member.MemberId && // 確保是該會員的常用旅客
                                                       ft.Status == FavoriteStatus.Active); // 確保常用旅客是有效的

                        if (favoriteTraveler == null)
                        {
                            return BadRequest($"找不到常用旅客 ID: {participantDto.FavoriteTravelerId.Value} 或該旅客已停用。");
                        }

                        // 使用常用旅客資料預填 OrderParticipant
                        participant.Name = favoriteTraveler.Name;
                        participant.BirthDate = favoriteTraveler.BirthDate ?? default; // 如果常用旅客生日可為null，提供預設值
                        participant.IdNumber = favoriteTraveler.IdNumber;
                        participant.Gender = favoriteTraveler.Gender ?? default; // 同上
                        participant.Phone = favoriteTraveler.Phone;
                        participant.Email = favoriteTraveler.Email;
                        participant.DocumentType = favoriteTraveler.DocumentType ?? default; // 同上
                        participant.DocumentNumber = favoriteTraveler.DocumentNumber;
                        participant.PassportSurname = favoriteTraveler.PassportSurname;
                        participant.PassportGivenName = favoriteTraveler.PassportGivenName;
                        participant.PassportExpireDate = favoriteTraveler.PassportExpireDate;
                        participant.Nationality = favoriteTraveler.Nationality;
                        participant.Note = favoriteTraveler.Note; // 可以考慮是否要合併 DTO 的 Note

                    }
                    order.OrderParticipants.Add(participant);
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
                    // 在第一次 SaveChangesAsync 之前賦值 MerchantTradeNo (不依賴 OrderId 的版本)
                    // 或者，如果 MerchantTradeNo 必須包含 OrderId，則需要在第一次 SaveChangesAsync 之後再更新一次。
                    // 這裡採用先產生一個基於時間戳和GUID片段的唯一編號，不依賴 OrderId。
                    // 如果您的 ECPayService.GenerateEcPayPaymentForm 中的 MerchantTradeNo 生成邏輯更完善，可以考慮將其提取為共用方法。
                    string tempGuidPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(); // 增加唯一性
                    string timePartForMtn = DateTime.UtcNow.ToString("yyMMddHHmmss"); // 使用 UTC 時間更佳
                    string prefixForMtn = "TRV";
                    string mtnBase = $"{prefixForMtn}{timePartForMtn}{tempGuidPart}";
                    order.MerchantTradeNo = new string(mtnBase.Where(char.IsLetterOrDigit).ToArray());
                    if (order.MerchantTradeNo.Length > 20)
                    {
                        order.MerchantTradeNo = order.MerchantTradeNo.Substring(0, 20);
                    }
                    _logger.LogInformation("為新訂單產生的 MerchantTradeNo: {MerchantTradeNo}", order.MerchantTradeNo);

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    // (可選) 如果 MerchantTradeNo 強烈建議包含 OrderId，可以在此處更新並再次儲存
                    bool requiresMtnUpdateWithOrderId = false; // 根據您的需求設定此標誌
                    if (requiresMtnUpdateWithOrderId)
                    {
                        string orderIdPartForMtn = order.OrderId.ToString("D5"); // 例如補零到5位
                        string finalMtnBase = $"{prefixForMtn}{orderIdPartForMtn}{timePartForMtn}"; // 假設 timePartForMtn 已在上面定義
                        order.MerchantTradeNo = new string(finalMtnBase.Where(char.IsLetterOrDigit).ToArray());
                        if (order.MerchantTradeNo.Length > 20)
                        {
                            order.MerchantTradeNo = order.MerchantTradeNo.Substring(0, 20);
                        }
                        _context.Orders.Update(order); // 標記為更新
                        await _context.SaveChangesAsync(); // 第二次儲存以更新 MerchantTradeNo
                        _logger.LogInformation("已使用 OrderId 更新訂單 MerchantTradeNo: {MerchantTradeNo}", order.MerchantTradeNo);
                    }
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
                                    existingFavTraveler.UpdatedAt = DateTime.UtcNow;
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
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
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
                OrderStatus = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                SelectedPaymentMethod = order.PaymentMethod?.ToString(),

            };
            return Created($"/api/Order/{order.OrderId}", orderSummary); // 假設之後會有GET api/Order/{id}
        }

        // *** 在這裡添加電話號碼正規化的私有 Helper Method ***
        private string NormalizePhoneNumber(string phoneNumberString)
        {
            if (string.IsNullOrEmpty(phoneNumberString))
            {
                return phoneNumberString; // 如果是 null 或空字串，直接返回
            }

            string normalizedNumber = phoneNumberString.Trim(); // 移除前後空白

            // 檢查是否以 "+" 開頭，且長度足夠進行檢查 (至少包含 +國碼數字0數字 或 +國碼數字數字)
            if (normalizedNumber.StartsWith("+") && normalizedNumber.Length >= 3) // 假設最短國碼數字至少2位，如 +886
            {
                // 找到國碼後面的數字部分
                int firstDigitAfterPlus = 1; // 從 '+' 的下一個字元開始
                int firstNonDigitIndexAfterPlus = firstDigitAfterPlus;

                // 找到國碼數字部分的結束位置 (第一個非數字字元)
                while (firstNonDigitIndexAfterPlus < normalizedNumber.Length && char.IsDigit(normalizedNumber[firstNonDigitIndexAfterPlus]))
                {
                    firstNonDigitIndexAfterPlus++;
                }

                // 如果國碼數字部分存在，且後面還有號碼
                if (firstNonDigitIndexAfterPlus > firstDigitAfterPlus && firstNonDigitIndexAfterPlus < normalizedNumber.Length)
                {
                    string countryCodeWithPlus = normalizedNumber.Substring(0, firstNonDigitIndexAfterPlus); // 包含 "+" 和國碼數字
                    string restOfNumber = normalizedNumber.Substring(firstNonDigitIndexAfterPlus); // 國碼後面的號碼部分

                    // 檢查號碼的其餘部分是否以 "0" 開頭，並且其長度大於 1
                    if (restOfNumber.StartsWith("0") && restOfNumber.Length > 1)
                    {
                        // 移除開頭的 "0"
                        restOfNumber = restOfNumber.Substring(1);
                    }

                    // 重新組合國碼和處理後的號碼
                    normalizedNumber = countryCodeWithPlus + restOfNumber;
                }
                // else: 如果格式不符合預期 (例如只有 + 或 +國碼沒有後續號碼)，保持原樣
            }
            // else: 如果不以 "+" 開頭，根據您的需求決定如何處理，這裡保持原樣

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
                .Include(o => o.OrderParticipants)
                .Include(o => o.OrderDetails)
                //    .ThenInclude(od => od.GroupTravel) // 如果需要在訂單詳情中顯示 GroupTravel 的資訊
                //.Include(o => o.OrderDetails)
                //    .ThenInclude(od => od.CustomTravel)   // 如果需要在訂單詳情中顯示 CustomTravel 的資訊
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
                            .FirstOrDefaultAsync(gt => gt.GroupTravelId == detail.ItemId);
                    }
                    else if (detail.Category == ProductCategory.CustomTravel)
                    {
                        // 使用 ItemId 從 _context.CustomTravels 查詢
                        // 確保 CustomTravel 實體有與 detail.ItemId 對應的主鍵，例如 CustomTravelId
                        detail.CustomTravel = await _context.CustomTravels
                            .FirstOrDefaultAsync(ct => ct.CustomTravelId == detail.ItemId);
                    }
                }
            }
            // 暫時回傳 order 實體，應建立並使用一個詳細的 OrderDto
            var orderDto = new // 你應該用你定義的 OrderDto
            {
                OrderId = orderData.OrderId,
                MemberId = orderData.MemberId,
                OrdererName = orderData.OrdererName,
                OrdererPhone = orderData.OrdererPhone,
                OrdererEmail = orderData.OrdererEmail,
                TotalAmount = orderData.TotalAmount,
                PaymentMethod = orderData.PaymentMethod?.ToString(),
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
                Participants = orderData.OrderParticipants?.Select(p => new OrderParticipantDto
                {
                    Name = p.Name,
                    BirthDate = p.BirthDate,
                    IdNumber = p.IdNumber,
                    Gender = p.Gender, // 轉換為字串
                    DocumentType = p.DocumentType // 轉換為字串
                }).ToList() ?? new List<OrderParticipantDto>(),
                OrderDetails = orderData.OrderDetails?.Select(od => new OrderDetailItemDto 
                {
                    //Category = od.Category, // DTO 中可能是 string
                    Category = od.Category, // Enum 可以直接賦值，如果 DTO 也是 Enum
                    ItemId = od.ItemId,
                    Description = od.Category == ProductCategory.GroupTravel ?
                                  od.GroupTravel?.OfficialTravelDetail?.OfficialTravel?.Title :
                                 (od.Category == ProductCategory.CustomTravel ? od.CustomTravel?.Note : od.Description), // 調整品名獲取
                    Quantity = od.Quantity,
                    Price = od.Price,
                    TotalAmount = od.TotalAmount
                    // ... 其他 OrderDetailItemDto 欄位 ...
                }).ToList() ?? new List<OrderDetailItemDto>(),
            };

            return Ok(orderDto);
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

    }

}