using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TravelAgencyBackend.Services;
using TravelAgency.Shared.Data;

namespace TravelAgencyBackend.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PermissionCheckService _perm;

        public HomeController(ILogger<HomeController> logger, PermissionCheckService perm)
            :base(perm)
        {
            _logger = logger;
            _perm = perm;
        }

        public IActionResult Index()
        {
            var check = CheckPermissionOrForbid("¬d¬Ý­º­¶");
            if (check != null) return check;

            ViewData["ActivePage"] = "Dashboard";
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

    }
}
