using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgencyFrontendAPI.DTOs.ChatRoomDTOs;

namespace TravelAgencyFrontendAPI.Controllers.ChatRoomControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StickersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StickersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/stickers
        [HttpGet]
        public IActionResult GetStickers()
        {
            var stickers = _context.Stickers
                .Select(s => new StickerDto
                {
                    Id = s.StickerId,
                    Url = s.ImagePath
                })
                .ToList();

            return Ok(stickers);
        }

        // GET: api/stickers/by-category
        [HttpGet("by-category")]
        public IActionResult GetByCategory(string category)
        {
            var stickers = _context.Stickers
                .Where(s => s.Category == category)
                .Select(s => new StickerDto
                {
                    Id = s.StickerId,
                    Url = s.ImagePath
                }).ToList();

            return Ok(stickers);
        }

        // GET: api/stickers/all-categories
        [HttpGet("all-categories")]
        public IActionResult GetAllCategories()
        {
            var categories = _context.Stickers
                .Select(s => s.Category)
                .Distinct()
                .ToList();

            return Ok(categories);
        }
    }
}
