using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.DTOs.ChatRoomDTOs;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyFrontendAPI.Hubs;

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
                .Include(c => c.Employee)
                .Select(c => new ChatRoomDto
                {
                    ChatRoomId = c.ChatRoomId,
                    EmployeeId = c.EmployeeId,
                    MemberId = c.MemberId,
                    IsBlocked = c.IsBlocked,
                    CreatedAt = c.CreatedAt,
                    LastMessageAt = c.LastMessageAt,
                    EmployeeName = c.Employee.Name
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

        [HttpGet("connection-id")]
        public ActionResult<string> GetConnectionId([FromQuery] string userType, [FromQuery] int userId)
        {
            var key = $"{userType}:{userId}";
            ChatHub.ConnectedUsers.UserToConnectionMap.TryGetValue(key, out var connId);

            if (string.IsNullOrEmpty(connId))
                return NotFound("對方未上線");

            return Ok(connId);
        }

        [HttpPost("start-with-default-cs")]
        public async Task<ActionResult<ChatRoomDto>> StartChatWithDefaultCustomerService([FromBody] StartChatRequest request)
        {
            var memberId = request.MemberId;

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Name == "客服人員");

            if (employee == null)
                return NotFound("找不到客服人員");

            var existing = await _context.ChatRooms
                .FirstOrDefaultAsync(c => c.EmployeeId == employee.EmployeeId && c.MemberId == memberId);

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
                EmployeeId = employee.EmployeeId,
                MemberId = memberId,
                IsBlocked = false,
                CreatedAt = DateTime.Now,
                LastMessageAt = null
            };

            _context.ChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();

            return Ok(new ChatRoomDto
            {
                ChatRoomId = chatRoom.ChatRoomId,
                EmployeeId = employee.EmployeeId,
                MemberId = memberId,
                IsBlocked = false,
                CreatedAt = chatRoom.CreatedAt,
                LastMessageAt = null
            });
        }

        [HttpPut("{id}/close")]
        public async Task<IActionResult> CloseChatRoom(int id)
        {
            var chatRoom = await _context.ChatRooms.FindAsync(id);
            if (chatRoom == null) return NotFound();

            chatRoom.Status = ChatStatus.Closed;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
