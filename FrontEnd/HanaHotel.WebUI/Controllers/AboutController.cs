using Microsoft.AspNetCore.Mvc;

namespace HanaHotel.WebUI.Controllers
{
    public class AboutController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Title = "Giới thiệu - Hana Hotel";
            return View();
        }
    }
}