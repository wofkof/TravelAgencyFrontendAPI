using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyBackend.Helpers;

namespace TravelAgencyBackend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var now = DateTime.Now;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);

            var totalMembers = _context.Members.Count();
            var totalOrders = _context.Orders.Count();

            var newMembersThisMonth = _context.Members.Count(m => m.RegisterDate >= thisMonthStart);
            var newOrdersThisMonth = _context.Orders.Count(o => o.CreatedAt >= thisMonthStart);
            var totalRevenueThisMonth = _context.Orders
                .Where(o => o.PaymentDate != null && o.PaymentDate >= thisMonthStart)
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;
            var averageRating = _context.Comments.Any() ? _context.Comments.Average(c => c.Rating) : 0;

            var rawOrderStats = _context.Orders
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .ToList();

            var monthlyOrderStats = rawOrderStats
                .Select(x => new { Month = $"{x.Year}-{x.Month.ToString("D2")}", Count = x.Count })
                .OrderBy(x => x.Month).ToList();

            var genderStats = _context.Members.GroupBy(m => m.Gender)
                .Select(g => new { Gender = g.Key, Count = g.Count() }).ToList();

            var paymentStats = _context.Orders.GroupBy(o => o.PaymentMethod)
                .Select(g => new { Method = g.Key, Count = g.Count() }).ToList();

            var orderStatusStats = _context.Orders.GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() }).ToList();

            var memberMonthlyStats = _context.Members
                .GroupBy(m => new { m.RegisterDate.Year, m.RegisterDate.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .ToList();

            var monthlyMemberStats = memberMonthlyStats
                .Select(x => new { Month = $"{x.Year}-{x.Month.ToString("D2")}", Count = x.Count })
                .OrderBy(x => x.Month).ToList();

            var ratingStats = _context.Comments
                .GroupBy(c => c.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .OrderBy(g => g.Rating).ToList();

            var travelStatusStats = _context.CustomTravels.GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() }).ToList();

            var officialTravelStatusStats = _context.OfficialTravels.GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() }).ToList();

            var officialTravelMonthlyStats = _context.OfficialTravels
                .GroupBy(o => new { o.CreatedAt.Value.Year, o.CreatedAt.Value.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .ToList();

            var monthlyOfficialTravelStats = officialTravelMonthlyStats
                .Select(x => new { Month = $"{x.Year}-{x.Month.ToString("D2")}", Count = x.Count })
                .OrderBy(x => x.Month).ToList();

            var groupStatusStats = _context.GroupTravels
                .GroupBy(g => g.GroupStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            var customTravelMonthlyStats = _context.CustomTravels
                .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
                .Select(g => new {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                }).ToList();

            var monthlyCustomTravelStats = customTravelMonthlyStats
                .Select(x => new {
                    Month = $"{x.Year}-{x.Month.ToString("D2")}",
                    Count = x.Count
                }).OrderBy(x => x.Month).ToList();

            var announcements = _context.Announcements
                .Where(a => a.Status == AnnouncementStatus.Published)
                .Include(a => a.Employee)
                .OrderByDescending(a => a.SentAt)
                .Take(5)
                .Select(a => new {
                    a.Title,
                    a.Content,
                    a.SentAt,
                    Employee = new { a.Employee.Name }
                }).ToList();

            ViewBag.Announcements = announcements;

            ViewBag.CustomTravelMonthLabels = monthlyCustomTravelStats.Select(x => x.Month).ToList();
            ViewBag.CustomTravelMonthCounts = monthlyCustomTravelStats.Select(x => x.Count).ToList();


            ViewBag.TotalMembers = totalMembers;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.NewMembersThisMonth = newMembersThisMonth;
            ViewBag.NewOrdersThisMonth = newOrdersThisMonth;
            ViewBag.TotalRevenueThisMonth = totalRevenueThisMonth;
            ViewBag.AverageRating = averageRating;

            ViewBag.OrderMonthLabels = monthlyOrderStats.Select(x => x.Month).ToList();
            ViewBag.OrderMonthCounts = monthlyOrderStats.Select(x => x.Count).ToList();

            ViewBag.GenderLabels = genderStats.Select(x => x.Gender.HasValue ? EnumDisplayHelper.GetDisplayName(x.Gender.Value) : "¥¼¶ñ¼g").ToList();
            ViewBag.GenderCounts = genderStats.Select(x => x.Count).ToList();

            ViewBag.PaymentLabels = paymentStats.Select(x => x.Method.HasValue ? EnumDisplayHelper.GetDisplayName(x.Method.Value) : "¥¼¶ñ¼g").ToList();
            ViewBag.PaymentCounts = paymentStats.Select(x => x.Count).ToList();

            ViewBag.OrderStatusLabels = orderStatusStats.Select(x => EnumDisplayHelper.GetDisplayName(x.Status)).ToList();
            ViewBag.OrderStatusCounts = orderStatusStats.Select(x => x.Count).ToList();

            ViewBag.MemberMonthLabels = monthlyMemberStats.Select(x => x.Month).ToList();
            ViewBag.MemberMonthCounts = monthlyMemberStats.Select(x => x.Count).ToList();

            ViewBag.RatingLabels = ratingStats.Select(x => x.Rating.ToString()).ToList();
            ViewBag.RatingCounts = ratingStats.Select(x => x.Count).ToList();

            ViewBag.TravelStatusLabels = travelStatusStats.Select(x => EnumDisplayHelper.GetDisplayName(x.Status)).ToList();
            ViewBag.TravelStatusCounts = travelStatusStats.Select(x => x.Count).ToList();

            ViewBag.OfficialTravelStatusLabels = officialTravelStatusStats.Select(x => EnumDisplayHelper.GetDisplayName(x.Status.Value)).ToList();
            ViewBag.OfficialTravelStatusCounts = officialTravelStatusStats.Select(x => x.Count).ToList();

            ViewBag.OfficialTravelMonthLabels = monthlyOfficialTravelStats.Select(x => x.Month).ToList();
            ViewBag.OfficialTravelMonthCounts = monthlyOfficialTravelStats.Select(x => x.Count).ToList();

            ViewBag.GroupStatusLabels = groupStatusStats.Select(x => x.Status ?? "¥¼¶ñ¼g").ToList();
            ViewBag.GroupStatusCounts = groupStatusStats.Select(x => x.Count).ToList();

            return View();
        }
    }
}
