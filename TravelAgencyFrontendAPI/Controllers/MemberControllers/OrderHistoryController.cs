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

        // GET: api/OrderHistory/list/{memberId}
        [HttpGet("list/{memberId}")]
        public async Task<IActionResult> GetOrderHistoryList(int memberId)
        {
            var orders = await _context.Orders
                .Where(o => o.MemberId == memberId && o.Status == OrderStatus.Completed)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderHistoryListItemDto
                {
                    OrderId = o.OrderId,
                    CreatedAt = o.CreatedAt,
                    Description = o.OrderDetails.FirstOrDefault().Description ?? "(無行程名稱)"
                })
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/OrderHistory/detail/{orderId}
        [HttpGet("detail/{orderId}")]
        public async Task<IActionResult> GetOrderHistoryDetail(int orderId)
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

    }
}
