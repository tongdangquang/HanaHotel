using Microsoft.AspNetCore.Mvc;

namespace HanaHotel.WebUI.ViewComponents.Room
{
    public class _RoomCoverPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            // Use an absolute path to the existing file
            return View("~/Views/Room/_RoomCoverPartial.cshtml");
        }
    }
}
