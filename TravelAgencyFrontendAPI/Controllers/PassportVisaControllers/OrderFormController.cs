using Microsoft.AspNetCore.Mvc;

namespace TravelAgencyFrontendAPI.Controllers.PassportVisaControllers
{
    public class OrderFormController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
