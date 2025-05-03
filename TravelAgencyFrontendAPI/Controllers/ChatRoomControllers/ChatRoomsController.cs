using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Data;
using TravelAgencyFrontendAPI.DTOs;
using TravelAgencyFrontendAPI.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelAgencyFrontendAPI.Controllers.ChatRoomControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatRoomsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ChatRoomsController(AppDbContext context) 
        {
            _context = context;
        }

        // GET: api/ChatRooms/{memberId}
        [HttpGet("{memberId}")]
        public async Task<ActionResult<IEnumerable<ChatRoomDto>>> GetChatRooms(int memberId)
        {
            var chatRooms = await _context.ChatRooms
                .Where(c => c.MemberId == memberId)
                .Select(c => new ChatRoomDto
                {
                    ChatRoomId = c.ChatRoomId,
                    EmployeeId = c.EmployeeId,
                    MemberId = c.MemberId,
                    IsBlocked = c.IsBlocked,
                    CreatedAt = c.CreatedAt,
                    LastMessageAt = c.LastMessageAt
                })
                .ToListAsync();

            return Ok(chatRooms);
        }

        // POST api/ChatRooms
        [HttpPost]
        public async Task<ActionResult<ChatRoomDto>> CreateChatRoom(ChatRoomDto dto)
        {
            var existing = await _context.ChatRooms
                .FirstOrDefaultAsync(c => c.EmployeeId == dto.EmployeeId && c.MemberId == dto.MemberId);

            if (existing != null) 
            {
                return Ok(new ChatRoomDto
                {
                    ChatRoomId = existing.ChatRoomId,
                    EmployeeId = existing.EmployeeId,
                    MemberId = existing.MemberId,
                    IsBlocked = existing.IsBlocked,
                    CreatedAt = existing.CreatedAt,
                    LastMessageAt = existing.LastMessageAt
                });
            }

            var chatRoom = new ChatRoom
            {
                EmployeeId = dto.EmployeeId,
                MemberId = dto.MemberId,
                IsBlocked = false,
                CreatedAt = DateTime.Now,
                LastMessageAt = null
            };

            _context.ChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();

            dto.ChatRoomId = chatRoom.ChatRoomId;
            dto.CreatedAt = chatRoom.CreatedAt;

            return CreatedAtAction(nameof(GetChatRooms), new { memberId = dto.MemberId }, dto);
        }

    }
}
