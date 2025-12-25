using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HanaHotel.WebUI.Controllers
{
    public class AdminLayoutController : Controller
    {
		[Authorize(Roles = "Admin")]
		public IActionResult _AdminLayout()
        {
            return View();
        }
    }
}

