using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TravelAgencyFrontendAPI.DTOs.ChatRoomDTOs;
using TravelAgencyFrontendAPI.Hubs;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.Controllers.ChatRoomControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageUploadController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IHubContext<ChatHub> _hub;

        public MessageUploadController(AppDbContext context, IWebHostEnvironment env, IHubContext<ChatHub> hub)
        {
            _context = context;
            _env = env;
            _hub = hub;
        }

        // POST: api/messageupload/upload-image
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage([FromForm] UploadImageFormDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("檔案不可為空");

            var ext = Path.GetExtension(dto.File.FileName).ToLower();
            var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            if (!allowedExts.Contains(ext))
                return BadRequest("只允許圖片格式");

            var fileName = $"{Guid.NewGuid()}{ext}";
            var saveDir = Path.Combine(_env.WebRootPath, "Uploads", "chat");
            Directory.CreateDirectory(saveDir);
            var savePath = Path.Combine(saveDir, fileName);

            await using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var relativePath = $"/Uploads/chat/{fileName}";

            // 儲存 Message 物件
            var message = new Message
            {
                ChatRoomId = dto.ChatRoomId,
                SenderType = dto.SenderType,
                SenderId = dto.SenderId,
                MessageType = MessageType.image,
                Content = relativePath,
                SentAt = DateTime.Now,
                IsRead = false
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync(); // 必須先儲存 Message 才有 MessageId

            // 儲存 MessageMedia 物件
            var media = new MessageMedia
            {
                MessageId = message.MessageId,
                MediaType = MediaType.image,
                FilePath = relativePath,
                DurationInSeconds = null
            };
            _context.MessageMedias.Add(media);
            await _context.SaveChangesAsync();

            // 廣播給聊天室用戶
            var msgDto = new MessageDto
            {
                MessageId = message.MessageId,
                ChatRoomId = message.ChatRoomId,
                SenderType = message.SenderType.ToString(),
                SenderId = message.SenderId,
                MessageType = message.MessageType.ToString(),
                Content = message.Content,
                SentAt = message.SentAt,
                IsRead = false
            };

            await _hub.Clients.Group(message.ChatRoomId.ToString())
                .SendAsync("ReceiveMessage", msgDto);

            return Ok(msgDto);
        }


        // POST: api/messageupload/upload-audio
        [HttpPost("upload-audio")]
        public async Task<IActionResult> UploadAudio([FromForm] UploadAudioFormDto dto)
        {
            if (dto.File == null || dto.File.Length == 0) return BadRequest("錄音時間過短");

            var ext = Path.GetExtension(dto.File.FileName).ToLower();
            var allowedExts = new[] { ".mp3", ".wav", ".ogg", ".webm" };
            if (!allowedExts.Contains(ext)) return BadRequest("只允許音訊格式");

            var fileName = $"{Guid.NewGuid()}{ext}";
            var saveDir = Path.Combine(_env.WebRootPath, "Uploads", "chat");
            Directory.CreateDirectory(saveDir);
            var savePath = Path.Combine(saveDir, fileName);

            await using (var stream = new FileStream(savePath, FileMode.Create)) 
            {
                await dto.File.CopyToAsync(stream);
            }

            var relativePath = $"/Uploads/chat/{fileName}";

            var message = new Message
            {
                ChatRoomId = dto.ChatRoomId,
                SenderType = dto.SenderType,
                SenderId = dto.SenderId,
                MessageType = MessageType.audio,
                Content = relativePath,
                SentAt = DateTime.Now,
                IsRead = false
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var media = new MessageMedia
            {
                MessageId = message.MessageId,
                MediaType = MediaType.audio,
                FilePath = relativePath,
                DurationInSeconds = dto.DurationInSeconds
            };
            _context.MessageMedias.Add(media);
            await _context.SaveChangesAsync();

            var msgDto = new MessageDto
            {
                MessageId = message.MessageId,
                ChatRoomId = message.ChatRoomId,
                SenderType = message.SenderType.ToString(),
                SenderId = message.SenderId,
                MessageType = message.MessageType.ToString(),
                Content = message.Content,
                SentAt = message.SentAt,
                IsRead = false
            };

            await _hub.Clients.Group(message.ChatRoomId.ToString())
                .SendAsync("ReceiveMessage", msgDto);

            return Ok(msgDto);
        }
    }
}
