using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyFrontendAPI.DTOs.CommentDTOs;

namespace TravelAgencyFrontendAPI.Controllers.Comments
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto dto)
        {
            // 檢查會員是否存在
            var member = await _context.Members.FindAsync(dto.MemberId);
            if (member == null)
            {
                return BadRequest("找不到該會員。");
            }

            // 查詢訂單明細，確認為該會員的已完成訂單
            var orderDetail = await _context.OrderDetails
                .Include(od => od.Order)
                .FirstOrDefaultAsync(od =>
                    od.OrderDetailId == dto.OrderDetailId &&
                    od.Order.MemberId == dto.MemberId &&
                    od.Order.Status == OrderStatus.Completed &&
                    od.EndDate != null &&
                    od.EndDate < DateTime.Now);

            if (orderDetail == null)
            {
                return BadRequest("該行程尚未完成或尚未結束，無法評論。");
            }

            // 防止重複評論
            var alreadyCommented = await _context.Comments.AnyAsync(c =>
                c.MemberId == dto.MemberId &&
                c.OrderDetailId == dto.OrderDetailId);

            if (alreadyCommented)
            {
                return BadRequest("您已對此行程發表過評論。");
            }

            var comment = new Comment
            {
                MemberId = dto.MemberId,
                OrderDetailId = dto.OrderDetailId,
                Category = dto.Category,
                Rating = dto.Rating,
                Content = dto.Content,
                Status = CommentStatus.Visible,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "評論成功發表。" });
        }
    }

}
