using Microsoft.AspNetCore.Mvc;

namespace TravelAgencyFrontendAPI.Controllers.PassportVisaControllers
{
    public class DocumentMenuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
