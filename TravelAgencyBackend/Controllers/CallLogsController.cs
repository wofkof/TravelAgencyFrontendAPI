using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyBackend.Services;

namespace TravelAgencyBackend.Controllers
{
    public class CallLogsController : BaseController
    {
        private readonly AppDbContext _context;

        public CallLogsController(AppDbContext context) : base(null)
        {
            _context = context;
        }

        // 顯示特定員工的通話紀錄
        public async Task<IActionResult> Index()
        {
            int employeeId = GetCurrentEmployeeId();
            
            var logs = await _context.CallLogs
                .Include(c => c.ChatRoom)
                .Where(c => c.ReceiverType == ReceiverType.Employee && c.ReceiverId == employeeId)
                .OrderByDescending(c => c.StartTime)
                .ToListAsync();

            return View(logs);
        }
    }
}
