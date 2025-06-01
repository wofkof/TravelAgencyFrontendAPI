using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyFrontendAPI.DTOs.MemberDTOs;
using TravelAgencyFrontendAPI.DTOs.OrderDTOs;
using TravelAgencyFrontendAPI.DTOs.OrderHistoryDTOs;

namespace TravelAgencyFrontendAPI.Controllers.MemberControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderHistoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderHistoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/OrderHistory/list/{memberId}?statuses=Completed&statuses=Expired
        [HttpGet("list/{memberId}")]
        public async Task<IActionResult> GetOrderHistoryList(int memberId,
        [FromQuery] List<OrderStatus> statuses,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5)
        {
            try
            {
                var query = _context.Orders
                    .Where(o => o.MemberId == memberId);

                if (statuses != null && statuses.Any())
                {
                    query = query.Where(o => statuses.Contains(o.Status));
                }

                var totalCount = await query.CountAsync();

                var orders = await query
                    .Include(o => o.OrderDetails)
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((page - 1) * pageSize) 
                    .Take(pageSize)              
                    .Select(o => new 
                    {
                        Order = o,
                        MainDetailDesc = o.OrderDetails.OrderBy(d => d.OrderDetailId).Select(d => d.Description).FirstOrDefault(),
                        MainDetailEndDate = o.OrderDetails.OrderBy(d => d.OrderDetailId).Select(d => d.EndDate).FirstOrDefault(),
                        MainDetailCategory = o.OrderDetails.OrderBy(d => d.OrderDetailId).Select(d => d.Category).FirstOrDefault(),
                        MainDetailId = o.OrderDetails.OrderBy(d => d.OrderDetailId).Select(d => d.OrderDetailId).FirstOrDefault()
                    })
                    .Select(x => new OrderHistoryListItemDto
                    {
                        OrderId = x.Order.OrderId,
                        CreatedAt = x.Order.CreatedAt,
                        Description = x.MainDetailDesc ?? "(無行程名稱)",
                        Status = x.Order.Status.ToString(),
                        OriginalStatus = x.Order.Status,
                        ExpiresAt = x.Order.ExpiresAt,
                        TotalAmount = x.Order.TotalAmount,
                        MerchantTradeNo = x.Order.MerchantTradeNo,
                        EndDate = x.MainDetailEndDate,
                        Category = x.MainDetailCategory.ToString(),
                        OrderDetailId = x.MainDetailId,

                        IsCommented = _context.Comments.Any(c =>
                            c.OrderDetailId == x.MainDetailId &&
                            c.MemberId == memberId)
                    })
                    .ToListAsync();

                return Ok(new
                {
                    items = orders,
                    totalCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❗ GetOrderHistoryList 發生例外：" + ex.ToString());
                return StatusCode(500, "查詢訂單列表時發生錯誤，請稍後再試");
            }
        }

        // GET: api/OrderHistory/detail/{orderId}
        [HttpGet("detail/{orderId}")]
        public async Task<IActionResult> GetOrderHistoryDetail(int orderId)
        {
            try
            {
                var orderData = await _context.Orders
               .Include(o => o.OrderParticipants)
               .Include(o => o.OrderDetails)
               .Include(o => o.OrderInvoices)
               .FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (orderData == null)
                {
                    return NotFound($"找不到訂單 ID {orderId}");
                }

                var invoice = orderData.OrderInvoices.FirstOrDefault();

                var dto = new OrderHistoryDetailDisplayDto
                {
                    Description = orderData.OrderDetails.FirstOrDefault()?.Description ?? "(無描述)",
                    StartDate = orderData.OrderDetails.FirstOrDefault()?.StartDate,
                    EndDate = orderData.OrderDetails.FirstOrDefault()?.EndDate,
                    AdultCount = orderData.OrderParticipants.Count(p => p.BirthDate <= DateTime.Today.AddYears(-12)),
                    ChildCount = orderData.OrderParticipants.Count(p => p.BirthDate > DateTime.Today.AddYears(-12)),
                    OrdererName = orderData.OrdererName,
                    OrdererPhone = orderData.OrdererPhone,
                    OrdererEmail = orderData.OrdererEmail,
                    OrdererNationality = orderData.OrdererNationality, //柏亦新增
                    OrdererDocumentType = orderData.OrdererDocumentType.ToString(), //柏亦新增
                    OrdererDocumentNumber = orderData.OrdererDocumentNumber, //柏亦新增
                    Note = orderData.Note,
                    PaymentMethod = orderData.PaymentMethod.ToString(),
                    Status = orderData.Status.ToString(),
                    CreatedAt = orderData.CreatedAt,
                    PaymentDate = orderData.PaymentDate,
                    TotalAmount = orderData.TotalAmount,
                    Participants = orderData.OrderParticipants.Select(p => new OrderHistoryParticipantDto
                    {
                        Name = p.Name,
                        BirthDate = p.BirthDate,
                        Gender = p.Gender.ToString(),
                        Nationality = p.Nationality,
                        IdNumber = p.IdNumber,
                        DocumentType = p.DocumentType.ToString(),
                        DocumentNumber = p.DocumentNumber,
                        PassportSurname = p.PassportSurname,
                        PassportGivenName = p.PassportGivenName,
                        PassportExpireDate = p.PassportExpireDate,
                        Email = p.Email,
                        Phone = p.Phone,
                        Note = p.Note
                    }).ToList(),
                    Invoice = invoice == null ? null : new OrderHistoryInvoiceDto
                    {
                        InvoiceNumber = invoice.InvoiceNumber,
                        InvoiceStatus = invoice.InvoiceStatus.ToString(),
                        InvoiceType = invoice.InvoiceType.ToString(),
                        BuyerName = invoice.BuyerName,
                        BuyerUniformNumber = invoice.BuyerUniformNumber,
                        TotalAmount = invoice.TotalAmount,
                        CreatedAt = invoice.CreatedAt,
                        InvoiceFileURL = invoice.InvoiceFileURL,
                        Note = invoice.Note
                    }
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❗ GetOrderHistoryDetail 發生例外：" + ex.ToString());
                return StatusCode(500, "查詢訂單詳情時發生錯誤，請稍後再試");
            }         
        }
    }
}
