using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyFrontendAPI.DTOs.ChatRoomDTOs;

namespace TravelAgencyFrontendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CallLogsController(AppDbContext context)
        {
            _context = context;
        }

        // 建立通話紀錄
        [HttpPost]
        public async Task<IActionResult> CreateCallLog([FromBody] CallLogDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var callLog = new CallLog
            {
                ChatRoomId = dto.ChatRoomId,
                CallerId = dto.CallerId,
                ReceiverId = dto.ReceiverId,
                CallerType = dto.CallerType,
                ReceiverType = dto.ReceiverType,
                CallType = dto.CallType,
                Status = dto.Status,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                DurationInSeconds = dto.DurationInSeconds
            };

            _context.CallLogs.Add(callLog);
            await _context.SaveChangesAsync();

            return Ok(callLog.CallId);
        }


        // 查詢特定聊天室的通話紀錄
        [HttpGet("ByChatRoom/{chatRoomId}")]
        public async Task<IActionResult> GetLogsByChatRoom(int chatRoomId)
        {
            var logs = await _context.CallLogs
                .Where(c => c.ChatRoomId == chatRoomId)
                .OrderByDescending(c => c.StartTime)
                .Select(c => new CallLog
                {
                    ChatRoomId = c.ChatRoomId,
                    CallerType = c.CallerType,
                    CallerId = c.CallerId,
                    ReceiverType = c.ReceiverType,
                    ReceiverId = c.ReceiverId,
                    CallType = c.CallType,
                    Status = c.Status,
                    StartTime = c.StartTime ?? DateTime.MinValue,
                    EndTime = c.EndTime,
                    DurationInSeconds = c.DurationInSeconds
                })
                .ToListAsync();

            return Ok(logs);
        }
    }
}
