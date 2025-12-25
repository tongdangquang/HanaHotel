using Microsoft.AspNetCore.Mvc;
using HanaHotel.BusinessLayer.Abstract;

namespace HanaHotel.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardWidgetController : ControllerBase
    {
        private readonly IGuestService _guestService;
        private readonly IRoomService _roomService;
        private readonly IBookingService _bookingService;

        public DashboardWidgetController(IGuestService guestService, IRoomService roomService, IBookingService bookingService)
        {
            _guestService = guestService;
            _roomService = roomService;
            _bookingService = bookingService;
        }

        [HttpGet]
        public IActionResult GetDashboardWidgetCounts()
        {

            var guest = _guestService.TGetList();
            var rooms = _roomService.TGetList();
            var bookings = _bookingService.TGetList();

            var widgetCounts = new Dictionary<string, string>
            {
                { "Guests", guest.Count.ToString() },
                { "Rooms", rooms.Count.ToString() },
                { "Bookings", bookings.Count.ToString() }
            };

            return Ok(widgetCounts);
        }
    }
}
