using Microsoft.AspNetCore.Mvc;

namespace HanaHotel.WebUI.ViewComponents.Room
{
    public class RoomCoverViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}