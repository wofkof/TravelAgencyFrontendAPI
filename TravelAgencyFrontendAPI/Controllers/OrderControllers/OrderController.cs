// File: Controllers/OrderController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // For ToListAsync, Include, etc.
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyFrontendAPI.DTOs.OrderDTOs; // Your Order DTOs namespace
using System.Security.Claims; // For HttpContext.User to get MemberId
using Microsoft.AspNetCore.Authorization; // If you want to protect this endpoint

namespace TravelAgencyFrontendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // <--- 加上這個，確保只有登入會員才能下單
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderController> _logger; // For logging

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

            // 1. 獲取當前登入的 MemberId
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("使用者未登入或無法識別身份。");
            }

            // 2. 驗證 Member 是否存在 (可選，但建議)
            var memberExists = await _context.Members.AnyAsync(m => m.MemberId == currentUserId.Value);
            if (!memberExists)
            {
                return NotFound($"會員 ID {currentUserId.Value} 不存在。");
            }

            // 3. 開始建立 Order 實體
            var order = new Order
            {
                MemberId = currentUserId.Value,

                OrdererName = orderCreateDto.OrdererInfo.Name, // 訂購人姓名
                OrdererPhone = NormalizePhoneNumber(orderCreateDto.OrdererInfo.MobilePhone), // 訂購人電話
                OrdererEmail = orderCreateDto.OrdererInfo.Email, // 訂購人Email

                TotalAmount = orderCreateDto.TotalAmount,
                PaymentMethod = orderCreateDto.SelectedPaymentMethod, // 使用者選擇的付款方式
                Status = OrderStatus.Pending, // 初始狀態設為 Pending (或 AwaitingPayment)
                CreatedAt = DateTime.UtcNow, // 使用 UTC 時間
                Note = orderCreateDto.OrderNotes,

                // 處理訂購人資訊 (即使從會員資料帶入，也記錄訂單當下的快照)
                // 這裡假設 Order Model 本身不直接儲存訂購人的姓名/電話/Email，
                // 這些資訊主要體現在 OrderParticipant (如果訂購人也是旅客)
                // 或者您可以考慮在 Order Model 新增 OrdererName, OrdererPhone, OrdererEmail 欄位
                // 以下是假設 Order Model 有這些欄位的情況：
                // OrdererName = orderCreateDto.OrdererInfo.Name,
                // OrdererPhone = orderCreateDto.OrdererInfo.MobilePhone,
                // OrdererEmail = orderCreateDto.OrdererInfo.Email,

                // 處理發票請求資訊
                InvoiceOption = orderCreateDto.InvoiceRequestInfo.InvoiceOption,
                InvoiceDeliveryEmail = orderCreateDto.InvoiceRequestInfo.InvoiceDeliveryEmail,
                InvoiceUniformNumber = orderCreateDto.InvoiceRequestInfo.InvoiceUniformNumber,
                InvoiceTitle = orderCreateDto.InvoiceRequestInfo.InvoiceTitle,
                InvoiceAddBillingAddr = orderCreateDto.InvoiceRequestInfo.InvoiceAddBillingAddr,
                InvoiceBillingAddress = orderCreateDto.InvoiceRequestInfo.InvoiceBillingAddress
                // 如果您為 "捐贈發票" 在 Order Model 新增了 IsInvoiceDonated 欄位:
                // IsInvoiceDonated = orderCreateDto.InvoiceRequestInfo.DonateInvoice,
            };

            // 4. 處理旅客列表 (OrderParticipants)
            foreach (var participantDto in orderCreateDto.Participants)
            {
                if (participantDto.DocumentType == DocumentType.Passport && string.IsNullOrEmpty(participantDto.DocumentNumber))
                {
                    // 添加一個 Model 錯誤或直接返回 BadRequest
                    ModelState.AddModelError($"Participants[{order.OrderParticipants.Count}].DocumentNumber", "選擇護照作為證件類型時，護照號碼為必填。");
                    // 如果您有多個自定義驗證，可以先收集錯誤，最後統一返回 BadRequest
                    // 為了範例簡潔，這裡假設您會統一處理錯誤
                }

                // 檢查必填欄位是否有值 (雖然 DTO 已有 [Required]，但這裡可以再次確認或記錄)
                if (string.IsNullOrEmpty(participantDto.Name)) { /* ... 錯誤處理 ... */ }
                // ... 其他必填欄位檢查 ...

                // 如果有自定義驗證錯誤，建議在這裡檢查並提前返回
                if (!ModelState.IsValid)
                {
                    // 記錄錯誤並返回
                    _logger.LogWarning("CreateOrder Participant Custom Validation Failed: {@ModelStateErrors}", ModelState);
                    return BadRequest(ModelState);
                }

                var participant = new OrderParticipant();

                if (participantDto.FavoriteTravelerId.HasValue && participantDto.FavoriteTravelerId.Value > 0)
                {
                    // 嘗試從常用旅客讀取資料
                    var favoriteTraveler = await _context.MemberFavoriteTravelers
                        .FirstOrDefaultAsync(ft => ft.FavoriteTravelerId == participantDto.FavoriteTravelerId.Value &&
                                                   ft.MemberId == currentUserId.Value && // 確保是該會員的常用旅客
                                                   ft.Status == FavoriteStatus.Active); // 確保常用旅客是有效的

                    if (favoriteTraveler != null)
                    {
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

                        // 記錄是從哪個常用旅客來的 (可選)
                        // participant.SourceFavoriteTravelerId = favoriteTraveler.FavoriteTravelerId;
                    }
                    else
                    {
                        // 找不到有效的常用旅客，記錄警告，並完全依賴 DTO 的資料
                        _logger.LogWarning($"找不到 MemberId: {currentUserId.Value} 的常用旅客 FavoriteTravelerId: {participantDto.FavoriteTravelerId.Value}，或該旅客狀態非 Active。將直接使用 DTO 提供的旅客資料。");
                    }
                }

                if (participantDto.DocumentType == DocumentType.Passport && string.IsNullOrEmpty(participantDto.DocumentNumber))
                {
                    ModelState.AddModelError($"Participants[{order.OrderParticipants.Count}].DocumentNumber", "選擇護照作為證件類型時，護照號碼為必填。");
                }
                // 或者其他組合規則，例如：IdNumber 或 DocumentNumber 至少要有一個非空？
                if (string.IsNullOrEmpty(participantDto.IdNumber) && string.IsNullOrEmpty(participantDto.DocumentNumber))
                {
                    ModelState.AddModelError($"Participants[{order.OrderParticipants.Count}].IdOrDocumentNumber", "旅客身分證號或證件號碼至少需填寫一項。");
                }
                // 無論是否從常用旅客載入，都允許 DTO 中的資料覆蓋或提供 (如果 DTO 欄位有值)
                // 這裡的邏輯是：如果 DTO 提供了值，就用 DTO 的；否則，如果從常用旅客載入了值，就保留。
                // 另一種策略是：常用旅客優先，DTO僅用於新增或沒有常用旅客ID的情況。
                // 以下採用 DTO 優先的策略 (如果 DTO 有值就覆蓋常用旅客的值)
                // 但對於 Name, Phone, Email 等必填欄位，如果常用旅客沒填到，DTO 應提供。

                // 確保 Name, Phone, Email 等核心資訊來自 DTO (因為 DTO 這些欄位是 Required)

                // 對於可選欄位，如果 DTO 有提供就使用，否則保留從常用旅客載入的值 (如果有的話)
                // DateTime 和 Enum 的 Nullable 判斷比較 tricky，因為 DTO 中通常直接是 DateTime/Enum 而不是 DateTime?/Enum?
                // 但 OrderParticipantDto 中的 BirthDate, Gender, DocumentType 是必填的
                participant.Name = participantDto.Name;
                participant.BirthDate = participantDto.BirthDate;
                participant.IdNumber = participantDto.IdNumber; // IdNumber 在 DTO 中允許為空
                participant.Gender = participantDto.Gender;
                participant.Phone = participantDto.Phone; // Phone 在 DTO 中允許為空
                participant.Email = participantDto.Email; // Email 在 DTO 中允許為空
                participant.DocumentType = participantDto.DocumentType; // DocumentType 在 DTO 中是必填
                participant.DocumentNumber = participantDto.DocumentNumber; // DocumentNumber 在 DTO 中允許為空

                // 可選的護照等欄位，如果 DTO 有提供就使用
                participant.PassportSurname = participantDto.PassportSurname;
                participant.PassportGivenName = participantDto.PassportGivenName;
                participant.PassportExpireDate = participantDto.PassportExpireDate;
                participant.Nationality = participantDto.Nationality;
                participant.Note = participantDto.Note;

                order.OrderParticipants.Add(participant);
            }

            // 在嘗試儲存到資料庫之前，再次檢查 ModelState，包括剛才添加的自定義錯誤
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("CreateOrder Final Model State Invalid After Custom Validation: {@ModelStateErrors}", ModelState);
                return BadRequest(ModelState);
            }

            // 5. 儲存到資料庫
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "建立訂單時資料庫更新失敗。");
                // 可以在這裡檢查 ex.InnerException 來獲取更詳細的資料庫層級錯誤 (例如欄位長度不符、Constraint 違規等)
                // 例如：System.Data.SqlClient.SqlException 或 Microsoft.EntityFrameworkCore.DbUpdateException 的 InnerException
                string innerMessage = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError("Inner Exception: {InnerMessage}", innerMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, "建立訂單失敗，請稍後再試。");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立訂單時發生未預期錯誤。");
                return StatusCode(StatusCodes.Status500InternalServerError, "建立訂單時發生錯誤。");
            }

            // 6. 準備回應給前端的資料
            // 回傳訂單ID，以及支付所需的相關資訊 (這部分取決於您與支付閘道的整合方式)
            var orderSummary = new OrderSummaryDto
            {
                OrderId = order.OrderId,
                OrderStatus = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                SelectedPaymentMethod = order.PaymentMethod?.ToString() ?? "N/A",
                // 您可能還需要回傳一個專門給支付閘道使用的 token 或重新導向 URL 的參數
                // PaymentGatewayRedirectUrl = ... (這需要根據支付閘道API決定)
                // PaymentReference = ...
            };

            // 使用 CreatedAtAction 回傳 201 Created，並提供新資源的 URI 和內容
            // 您需要有一個 GetOrder(int id) 的方法來讓 CreatedAtAction 正常運作
            // 為簡化，這裡先直接回傳 Ok 或 Created (不帶 Location Header)
            // return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, orderSummary);
            return Created($"/api/Order/{order.OrderId}", orderSummary); // 假設之後會有GET api/Order/{id}
        }


        // 輔助方法：獲取當前登入使用者的 MemberId
        // 實際實作會根據您的身份驗證設定而有所不同
        private int? GetCurrentUserId()
        {
            return 11110;
            //var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            //if (int.TryParse(userIdClaim, out int userId))
            //{
            //    return userId;
            //}

            //// var memberIdClaim = User.FindFirstValue("MemberId");
            //// if (int.TryParse(memberIdClaim, out int memberId)) return memberId;

            //_logger.LogWarning("無法從 Claims 中解析 UserId。");
            //return null;
        }

        // GET: api/Order/{id} (範例，用於 CreatedAtAction 和前端查詢訂單)
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id) //  OrderDto 用於顯示訂單詳情
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            var order = await _context.Orders
                                .Include(o => o.OrderParticipants)
                                .Include(o => o.OrderDetails) // 付款完成後才會有資料
                                .FirstOrDefaultAsync(o => o.OrderId == id && o.MemberId == currentUserId.Value);

            if (order == null)
            {
                return NotFound($"找不到訂單 ID {id}，或您無權存取此訂單。");
            }

            // 這裡需要將 Order 實體映射到一個 OrderDto (包含訂單所有詳細資訊的 DTO)
            // 這個 OrderDto 會比 OrderSummaryDto 更詳細
            // 以下為簡化範例，直接回傳 order (實際應映射到 DTO)
            // var orderDto = MapOrderToOrderDto(order); // 您需要實作這個映射方法
            // return Ok(orderDto);

            // 暫時回傳 order 實體，您應建立並使用一個詳細的 OrderDto
            return Ok(new{
                order.OrderId,
                order.MemberId,
                OrdererName = order.OrdererName, // 回傳快照的訂購人姓名
                OrdererPhone = order.OrdererPhone, // 回傳快照的訂購人電話
                OrdererEmail = order.OrdererEmail, // 回傳快照的訂購人Email
                order.TotalAmount,
                PaymentMethod = order.PaymentMethod?.ToString(),
                Status = order.Status.ToString(),
                order.CreatedAt,
                order.PaymentDate,
                order.Note,
                order.InvoiceOption,
                order.InvoiceDeliveryEmail,
                order.InvoiceUniformNumber,
                order.InvoiceTitle,
                order.InvoiceAddBillingAddr,
                order.InvoiceBillingAddress,
                Participants = order.OrderParticipants.Select(p => new {
                    p.Name,
                    p.Phone,
                    p.Email, // 擷取部分旅客資訊
                    p.BirthDate,
                    p.IdNumber,
                    p.Gender,
                    p.DocumentType
                }).ToList(),
                OrderDetails = order.OrderDetails.Select(od => new {
                    od.Description,
                    od.Quantity,
                    od.Price,
                    od.TotalAmount
                }).ToList()
            });
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
    }



    // 定義這個 OrderSummaryDto，用於 PostOrder 的回應
    public class OrderSummaryDto
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public string SelectedPaymentMethod { get; set; }
        // public string PaymentGatewayRedirectUrl { get; set; } // 用於支付
        // public string PaymentReference { get; set; } // 
    }

    // 您可能還需要一個更詳細的 OrderDto，用於 GetOrderById
    // public class OrderDto { ... }
}