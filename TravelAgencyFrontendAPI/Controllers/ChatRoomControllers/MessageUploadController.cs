using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using TravelAgencyFrontendAPI.Data;
using TravelAgencyFrontendAPI.DTOs.ChatRoomDTOs;
using TravelAgencyFrontendAPI.Models;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelAgencyFrontendAPI.Controllers.ChatRoomControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageUploadController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MessageUploadController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage([FromForm] UploadImageFormDto dto)
        {
            if (dto.File == null || dto.File.Length == 0) return BadRequest("檔案不可為空");

            var ext = Path.GetExtension(dto.File.FileName).ToLower();
            var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            if (!allowedExts.Contains(ext.ToLower())) return BadRequest("只允許圖片格式");

            var fileName = $"{Guid.NewGuid()}{ext}";
            var savePath = Path.Combine(_env.WebRootPath, "Uploads", "chat");
            Directory.CreateDirectory(savePath);

            var filePath = Path.Combine(savePath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create)) 
            {
                await dto.File.CopyToAsync(stream);
            }

            var relativePath = $"/Uploads/chat/{fileName}";

            var message = new Message
            {
                ChatRoomId = dto.ChatRoomId,
                SenderType = dto.SenderType,
                SenderId = dto.SenderId,
                MessageType = dto.MessageType,
                Content = relativePath,
                SentAt = DateTime.Now,
                IsRead = false
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            if (message.MessageType != MessageType.image)return BadRequest("僅支援圖片類型");
            
            var media = new MessageMedia
            {
                MessageId = message.MessageId,
                MediaType = MediaType.image,
                FilePath = relativePath
            };

            return Ok(new { message.MessageId, relativePath });
        }
    }
}