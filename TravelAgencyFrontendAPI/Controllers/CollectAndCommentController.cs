using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyFrontendAPI.DTOs.CollectCommentDTOs;

namespace TravelAgencyFrontendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollectAndCommentController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CollectAndCommentController(AppDbContext context)
        {
            _context = context;
        }

        //讀取收藏
        [HttpPost("getMyCollections")]
        public async Task<ActionResult> GetMyCollections(int memberId)
        {
            if (memberId <= 0)
            {
                return BadRequest(new { message = "此會員不存在" });
            }

            try 
            {
                var collections = await (
                from c in _context.Collects
                where c.MemberId == memberId
                select new getMyCollectionsDto
                {
                    MemberId = c.MemberId,
                    TravelId = c.TravelId,
                    TravelType = c.TravelType,
                    CollectId = c.CollectId,
                    title = c.TravelType == CollectType.Custom ?
                            _context.CustomTravels.Where(ct => ct.CustomTravelId == c.TravelId).Select(ct => ct.Note).FirstOrDefault() :
                            _context.OfficialTravels.Where(ot => ot.OfficialTravelId == c.TravelId).Select(ot => ot.Title).FirstOrDefault(),
                    Description = c.TravelType == CollectType.Custom ?
                             "" :_context.OfficialTravels.Where(ot => ot.OfficialTravelId == c.TravelId).Select(ot => ot.Description).FirstOrDefault()

                }
                ).ToListAsync();
                if (collections == null || collections.Count == 0)
                {
                    return NotFound("暫無收藏行程");
                }

                return Ok(collections);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "伺服器錯誤", error = ex.Message });
            }

            
        }

        //讀取評論
        [HttpPost("getMyComments")]
        public async Task<ActionResult> GetMyComments(int memberId) 
        {
            if (memberId <= 0)
            {
                return BadRequest(new { message = "此會員不存在" });
            }
            try 
            {
                var comments = await (
                    from c in _context.Comments
                    where c.MemberId == memberId && c.Status != CommentStatus.Deleted
                    select new getMyCommentsDto
                    {
                        MemberId = c.MemberId,
                        TravelId = c.TravelId,
                        TravelType = c.TravelType,
                        CommentId = c.CommentId,
                        Rating = c.Rating,
                        Content = c.Content,
                        Status = c.Status,
                        title = c.TravelType == CommentType.Custom ?
                                _context.CustomTravels.Where(ct => ct.CustomTravelId == c.TravelId).Select(ct => ct.Note).FirstOrDefault() :
                                _context.OfficialTravels.Where(ot => ot.OfficialTravelId == c.TravelId).Select(ot => ot.Title).FirstOrDefault(),
                        Description = c.TravelType == CommentType.Custom ?
                                 "" : _context.OfficialTravels.Where(ot => ot.OfficialTravelId == c.TravelId).Select(ot => ot.Description).FirstOrDefault()
                    }
                    ).ToListAsync();
                if (comments == null || comments.Count == 0)
                {
                    return NotFound("暫無評論");
                }
                return Ok(comments);
            } 
            catch (Exception ex) 
            {
                return StatusCode(500, new { message = "伺服器錯誤", error = ex.Message });
            }

        }

        //加入收藏
        //取消收藏
        [HttpDelete("deleteCollection")]
        public async Task<ActionResult> DeleteCollection(int collectId)
        {
            if (collectId <= 0)
            {
                return BadRequest(new { message = "收藏不存在" });
            }
            var collect = await _context.Collects.FindAsync(collectId);
            if (collect == null)
            {
                return NotFound(new { message = "收藏不存在" });
            }
            _context.Collects.Remove(collect);
            await _context.SaveChangesAsync();
            return Ok(new { message = "收藏已刪除" });
        }
        //新增評論
        //修改評論
        //刪除評論
        [HttpDelete("deleteComment")]
        public async Task<ActionResult> DeleteComment(int commentId)
        {
            if (commentId <= 0)
            {
                return BadRequest(new { message = "評論不存在" });
            }
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound(new { message = "評論不存在" });
            }
            comment.Status = CommentStatus.Deleted;
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
            return Ok(new { message = "評論已刪除" });

        }
    }
}
