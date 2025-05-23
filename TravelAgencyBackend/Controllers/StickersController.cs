using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.Controllers
{
    public class StickersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StickersController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 單頁管理介面
        public async Task<IActionResult> Manage()
        {
            var stickers = await _context.Stickers.ToListAsync();
            return View(stickers);
        }

        // 上傳貼圖
        [HttpPost]
        public async Task<IActionResult> Create(IFormFile file, string category)
        {
            if (file == null || file.Length == 0)
                return BadRequest("請選擇檔案");

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest("檔案類型錯誤");

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(ext))
                return BadRequest("副檔名錯誤");

            var uploads = Path.Combine(_env.WebRootPath, "uploads/stickers");
            Directory.CreateDirectory(uploads);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/stickers/{fileName}";

            var sticker = new Sticker
            {
                Category = category,
                ImagePath = imageUrl
            };

            _context.Stickers.Add(sticker);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                sticker.StickerId,
                sticker.Category,
                sticker.ImagePath
            });
        }

        // 刪除貼圖
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var sticker = await _context.Stickers.FindAsync(id);
            if (sticker == null) return NotFound();

            // 刪除圖檔
            var fullPath = Path.Combine(_env.WebRootPath, sticker.ImagePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            _context.Stickers.Remove(sticker);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // 編輯分類（僅分類欄位）
        [HttpPost]
        public async Task<IActionResult> EditCategory(int id, [FromBody] CategoryUpdateDto dto)
        {
            var sticker = await _context.Stickers.FindAsync(id);
            if (sticker == null) return NotFound();

            sticker.Category = dto.Category;
            await _context.SaveChangesAsync();
            return Ok();
        }

        public class CategoryUpdateDto
        {
            public string Category { get; set; } = string.Empty;
        }
    }
}
