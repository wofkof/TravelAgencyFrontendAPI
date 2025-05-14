using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Data;
using TravelAgencyFrontendAPI.DTOs.ChatRoomDTOs;
using TravelAgencyFrontendAPI.Hubs;
using TravelAgencyFrontendAPI.Models;
using static TravelAgencyFrontendAPI.Hubs.ChatHub;

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

        // GET: api/messages/connection-id
        [HttpGet("connection-id")]
        public IActionResult GetConnectionId([FromQuery] string userType, [FromQuery] int userId)
        {
            var key = $"{userType}:{userId}";
            if (ChatHub.ConnectedUsers.UserToConnectionMap.TryGetValue(key, out var connectionId))
            {
                return Ok(connectionId);
            }

            return NotFound("使用者尚未連線");
        }

    }
}
