using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Data;
using TravelAgencyFrontendAPI.DTOs.ChatRoomDTOs;
using TravelAgencyFrontendAPI.Hubs;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Controllers.ChatRoomControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IHubContext<ChatHub> _hub;

        public MessagesController(AppDbContext context, IWebHostEnvironment env, IHubContext<ChatHub> hub)
        {
            _context = context;
            _env = env;
            _hub = hub;
        }

        // GET: api/Messages/{chatRoomId}
        [HttpGet("{chatRoomId}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(int chatRoomId)
        {
            var messages = await _context.Messages
                .Where(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageDto
                {
                    MessageId = m.MessageId,
                    ChatRoomId = m.ChatRoomId,
                    SenderType = m.SenderType.ToString(),
                    SenderId = m.SenderId,
                    MessageType = m.MessageType.ToString(),
                    Content = m.Content,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead
                })
                .ToListAsync();

            return Ok(messages);
        }

        // POST: api/Messages
        [HttpPost]
        public async Task<ActionResult<MessageDto>> SendMessage(MessageDto dto)
        {
            var message = new Message
            {
                ChatRoomId = dto.ChatRoomId,
                SenderType = Enum.Parse<SenderType>(dto.SenderType, true),
                SenderId = dto.SenderId,
                MessageType = Enum.Parse<MessageType>(dto.MessageType, true),
                Content = dto.Content,
                SentAt = DateTime.Now,
                IsRead = false,
                IsDeleted = false
            };

            _context.Messages.Add(message);

            var chatRoom = await _context.ChatRooms.FindAsync(dto.ChatRoomId);
            if (chatRoom != null)
            {
                chatRoom.LastMessageAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            dto.MessageId = message.MessageId;
            dto.SentAt = message.SentAt;

            // SignalR 廣播
            await _hub.Clients.Group(dto.ChatRoomId.ToString())
                .SendAsync("ReceiveMessage", dto);

            return CreatedAtAction(nameof(GetMessages), new { chatRoomId = dto.ChatRoomId }, dto);
        }

        // POST: api/messages/upload-image
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


        // POST: api/messages/mark-as-read
        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadDto dto)
        {
            var senderTypeEnum = Enum.Parse<SenderType>(dto.SenderType, true);

            var unreadMessages = await _context.Messages
                .Where(m => m.ChatRoomId == dto.ChatRoomId
                    && !m.IsRead
                    && m.SenderType != senderTypeEnum
                    && m.SenderId != dto.SenderId)
                .ToListAsync();

            foreach (var msg in unreadMessages)
                msg.IsRead = true;

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
