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
    // [Authorize] // <--- 建議加上這個，確保只有登入會員才能下單
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

                //OrdererName = orderCreateDto.OrdererInfo.Name, // 訂購人姓名
                //OrdererPhone = orderCreateDto.OrdererInfo.MobilePhone, // 訂購人電話
                //OrdererEmail = orderCreateDto.OrdererInfo.Email, // 訂購人Email

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

                // 無論是否從常用旅客載入，都允許 DTO 中的資料覆蓋或提供 (如果 DTO 欄位有值)
                // 這裡的邏輯是：如果 DTO 提供了值，就用 DTO 的；否則，如果從常用旅客載入了值，就保留。
                // 另一種策略是：常用旅客優先，DTO僅用於新增或沒有常用旅客ID的情況。
                // 以下採用 DTO 優先的策略 (如果 DTO 有值就覆蓋常用旅客的值)
                // 但對於 Name, Phone, Email 等必填欄位，如果常用旅客沒填到，DTO 應提供。

                // 確保 Name, Phone, Email 等核心資訊來自 DTO (因為 DTO 這些欄位是 Required)
                participant.Name = participantDto.Name;
                participant.Phone = participantDto.Phone;
                participant.Email = participantDto.Email;

                // 對於可選欄位，如果 DTO 有提供就使用，否則保留從常用旅客載入的值 (如果有的話)
                // DateTime 和 Enum 的 Nullable 判斷比較 tricky，因為 DTO 中通常直接是 DateTime/Enum 而不是 DateTime?/Enum?
                // 但 OrderParticipantDto 中的 BirthDate, Gender, DocumentType 是必填的

                participant.BirthDate = participantDto.BirthDate;
                participant.IdNumber = participantDto.IdNumber; // DTO中 IdNumber 也是必填
                participant.Gender = participantDto.Gender;     // DTO中 Gender 也是必填
                participant.DocumentType = participantDto.DocumentType; // DTO中 DocumentType 也是必填

                // 可選的，如果DTO有值就覆蓋
                if (!string.IsNullOrEmpty(participantDto.DocumentNumber))
                    participant.DocumentNumber = participantDto.DocumentNumber;
                if (!string.IsNullOrEmpty(participantDto.PassportSurname))
                    participant.PassportSurname = participantDto.PassportSurname;
                if (!string.IsNullOrEmpty(participantDto.PassportGivenName))
                    participant.PassportGivenName = participantDto.PassportGivenName;
                if (participantDto.PassportExpireDate.HasValue)
                    participant.PassportExpireDate = participantDto.PassportExpireDate.Value;
                if (!string.IsNullOrEmpty(participantDto.Nationality))
                    participant.Nationality = participantDto.Nationality;
                if (!string.IsNullOrEmpty(participantDto.Note)) // 可以考慮合併常用旅客的備註和DTO的備註
                    participant.Note = participantDto.Note;


                // 如果 participantDto.MemberIdAsParticipant 有值，表示這位旅客同時也是系統會員
                // 您可以根據此 ID 做額外檢查或記錄，但 OrderParticipant 表本身沒有直接的 MemberId FK
                // participant.AssociatedMemberId = participantDto.MemberIdAsParticipant; (如果 OrderParticipant 有此欄位)

                order.OrderParticipants.Add(participant);
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
                // 可以檢查 ex.InnerException 來獲取更詳細的錯誤
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
                //OrdererName = order.OrdererName, // 回傳快照的訂購人姓名
                //OrdererPhone = order.OrdererPhone, // 回傳快照的訂購人電話
                //OrdererEmail = order.OrdererEmail, // 回傳快照的訂購人Email
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