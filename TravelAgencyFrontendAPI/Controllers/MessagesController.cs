using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Data;
using TravelAgencyFrontendAPI.DTOs;
using TravelAgencyFrontendAPI.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelAgencyFrontendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public MessagesController(AppDbContext context)
        {
            _context = context;
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
                SenderType = Enum.Parse<SenderType>(dto.SenderType),
                SenderId = dto.SenderId,
                MessageType = Enum.Parse<MessageType>(dto.MessageType),
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

            return CreatedAtAction(nameof(GetMessages), new { chatRoomId = dto.ChatRoomId }, dto);
        }
    }
}
