using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TravelAgency.Shared.Data;
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
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .ToList();

            var monthlyOrderStats = rawOrderStats
                .Select(x => new
                {
                    Month = $"{x.Year}-{x.Month.ToString("D2")}",
                    Count = x.Count
                })
                .OrderBy(x => x.Month)
                .ToList();

            var genderStats = _context.Members
                .GroupBy(m => m.Gender)
                .Select(g => new
                {
                    Gender = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var paymentStats = _context.Orders
                .GroupBy(o => o.PaymentMethod)
                .Select(g => new
                {
                    Method = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var orderStatusStats = _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var memberMonthlyStats = _context.Members
                .GroupBy(m => new { m.RegisterDate.Year, m.RegisterDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .ToList();

            var monthlyMemberStats = memberMonthlyStats
                .Select(x => new
                {
                    Month = $"{x.Year}-{x.Month.ToString("D2")}",
                    Count = x.Count
                })
                .OrderBy(x => x.Month)
                .ToList();

            var ratingStats = _context.Comments
                .GroupBy(c => c.Rating)
                .Select(g => new
                {
                    Rating = g.Key,
                    Count = g.Count()
                })
                .OrderBy(g => g.Rating)
                .ToList();

            var travelStatusStats = _context.CustomTravels
                .GroupBy(t => t.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToList();

            ViewBag.TotalMembers = totalMembers;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.NewMembersThisMonth = newMembersThisMonth;
            ViewBag.NewOrdersThisMonth = newOrdersThisMonth;
            ViewBag.TotalRevenueThisMonth = totalRevenueThisMonth;
            ViewBag.AverageRating = averageRating;

            ViewBag.OrderMonthLabels = monthlyOrderStats.Select(x => x.Month).ToList();
            ViewBag.OrderMonthCounts = monthlyOrderStats.Select(x => x.Count).ToList();

            ViewBag.GenderLabels = genderStats
                .Select(x => x.Gender.HasValue
                    ? EnumDisplayHelper.GetDisplayName(x.Gender.Value)
                    : "¥¼¶ñ¼g")
                .ToList();
            ViewBag.GenderCounts = genderStats.Select(x => x.Count).ToList();

            ViewBag.PaymentLabels = paymentStats
                .Select(x => x.Method.HasValue
                    ? EnumDisplayHelper.GetDisplayName(x.Method.Value)
                    : "¥¼¶ñ¼g")
                .ToList();
            ViewBag.PaymentCounts = paymentStats.Select(x => x.Count).ToList();

            ViewBag.OrderStatusLabels = orderStatusStats
                .Select(x => EnumDisplayHelper.GetDisplayName(x.Status))
                .ToList();
            ViewBag.OrderStatusCounts = orderStatusStats.Select(x => x.Count).ToList();

            ViewBag.MemberMonthLabels = monthlyMemberStats.Select(x => x.Month).ToList();
            ViewBag.MemberMonthCounts = monthlyMemberStats.Select(x => x.Count).ToList();

            ViewBag.RatingLabels = ratingStats.Select(x => x.Rating.ToString()).ToList();
            ViewBag.RatingCounts = ratingStats.Select(x => x.Count).ToList();

            ViewBag.TravelStatusLabels = travelStatusStats.Select(x => EnumDisplayHelper.GetDisplayName(x.Status)).ToList();
            ViewBag.TravelStatusCounts = travelStatusStats.Select(x => x.Count).ToList();

            return View();
        }
    }
}