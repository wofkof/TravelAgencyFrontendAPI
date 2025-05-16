using TravelAgency.Shared.Models;
using TravelAgency.Shared.Data;

namespace TravelAgencyBackend.ViewComponent
{
    public class DashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public int GetUnreadMessageCountForEmployee(int employeeId) 
        {
            return _context.Messages
                .Where(m => m.SenderType == SenderType.Member 
                            && !m.IsRead 
                            && m.ChatRoom.EmployeeId == employeeId)
                .Count();
        }
    }
}
