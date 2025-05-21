using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.DTOs.ChatRoomDTOs;
using TravelAgencyFrontendAPI.Hubs;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;

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
                .ToListAsync();

            var messageDtos = new List<MessageDto>();

            var memberIds = messages.Where(m => m.SenderType == SenderType.Member)
                                    .Select(m => m.SenderId).Distinct().ToList();
            var employeeIds = messages.Where(m => m.SenderType == SenderType.Employee)
                                      .Select(m => m.SenderId).Distinct().ToList();

            var memberMap = await _context.Members
                .Where(m => memberIds.Contains(m.MemberId))
                .ToDictionaryAsync(m => m.MemberId, m => m.Name);

            var employeeMap = await _context.Employees
                .Where(e => employeeIds.Contains(e.EmployeeId))
                .ToDictionaryAsync(e => e.EmployeeId, e => e.Name);

            foreach (var msg in messages)
            {
                var senderName = msg.SenderType == SenderType.Employee
                    ? employeeMap.GetValueOrDefault(msg.SenderId, "未知員工")
                    : memberMap.GetValueOrDefault(msg.SenderId, "未知會員");

                messageDtos.Add(new MessageDto
                {
                    MessageId = msg.MessageId,
                    ChatRoomId = msg.ChatRoomId,
                    SenderType = msg.SenderType.ToString(),
                    SenderId = msg.SenderId,
                    SenderName = senderName,
                    MessageType = msg.MessageType.ToString(),
                    Content = msg.Content,
                    SentAt = msg.SentAt,
                    IsRead = msg.IsRead
                });
            }

            return Ok(messageDtos);
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

            string senderName;
            if (message.SenderType == SenderType.Employee)
            {
                var emp = await _context.Employees.FindAsync(message.SenderId);
                senderName = emp?.Name ?? "未知員工";
            }
            else
            {
                var mem = await _context.Members.FindAsync(message.SenderId);
                senderName = mem?.Name ?? "未知會員";
            }

            var messageDto = new MessageDto
            {
                MessageId = message.MessageId,
                ChatRoomId = message.ChatRoomId,
                SenderId = message.SenderId,
                SenderType = message.SenderType.ToString(),
                SenderName = senderName,
                MessageType = message.MessageType.ToString(),
                Content = message.Content,
                SentAt = message.SentAt,
                IsRead = message.IsRead
            };

            // SignalR 廣播
            await _hub.Clients.Group(dto.ChatRoomId.ToString())
                .SendAsync("ReceiveMessage", messageDto);

            return CreatedAtAction(nameof(GetMessages), new { chatRoomId = dto.ChatRoomId }, messageDto);
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
