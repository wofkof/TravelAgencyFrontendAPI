using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyBackend.Services;
using TravelAgencyBackend.ViewModels;

namespace TravelAgencyBackend.Controllers
{
    public class CallLogsController : BaseController
    {
        private readonly AppDbContext _context;

        public CallLogsController(AppDbContext context) : base(null)
        {
            _context = context;
        }

        // 顯示通話紀錄
        public async Task<IActionResult> Index(string? keyword, Status? statusFilter, int page = 1, int pageSize = 10)
        {
            int employeeId = GetCurrentEmployeeId();

            var query = _context.CallLogs
                .Include(c => c.ChatRoom)
                .Where(c =>
                    (c.ReceiverType == ReceiverType.Employee && c.ReceiverId == employeeId) ||
                    (c.CallerType == CallerType.Employee && c.CallerId == employeeId)
                );

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var matchedEmployeeIds = await _context.Employees
                    .Where(e => e.Name.Contains(keyword))
                    .Select(e => e.EmployeeId)
                    .ToListAsync();

                var matchedMemberIds = await _context.Members
                    .Where(m => m.Name.Contains(keyword))
                    .Select(m => m.MemberId)
                    .ToListAsync();

                query = query.Where(c =>
                    c.CallerId.ToString().Contains(keyword) ||
                    c.ReceiverId.ToString().Contains(keyword) ||
                    matchedEmployeeIds.Contains(c.CallerId) ||
                    matchedEmployeeIds.Contains(c.ReceiverId) ||
                    matchedMemberIds.Contains(c.CallerId) ||
                    matchedMemberIds.Contains(c.ReceiverId)
                );
            }

            if (statusFilter.HasValue)
            {
                query = query.Where(c => c.Status == statusFilter);
            }

            var totalCount = await query.CountAsync();
            var logs = await query
                .OrderByDescending(c => c.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.EmployeeNames = await _context.Employees.ToDictionaryAsync(e => e.EmployeeId, e => e.Name);
            ViewBag.MemberNames = await _context.Members.ToDictionaryAsync(m => m.MemberId, m => m.Name);
            ViewBag.EmployeeId = employeeId;

            return View(new CallLogListViewModel
            {
                Logs = logs,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Keyword = keyword,
                StatusFilter = statusFilter
            });
        }

    }
}
