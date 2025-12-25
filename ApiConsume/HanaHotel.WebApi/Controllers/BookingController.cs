using Microsoft.AspNetCore.Mvc;
using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;
using HanaHotel.DataAccessLayer.Abstract;

namespace HanaHotel.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IRoomDetailDal _roomDetailDal;
        private readonly IRoomDal _roomDal;
        private readonly IHotelDetailDal _hotelDetailDal;
        private readonly IHotelDal _hotelDal;

        public BookingController(IBookingService bookingService,
                                 IRoomDetailDal roomDetailDal,
                                 IRoomDal roomDal,
                                 IHotelDetailDal hotelDetailDal,
                                 IHotelDal hotelDal)
        {
            _bookingService = bookingService;
            _roomDetailDal = roomDetailDal;
            _roomDal = roomDal;
            _hotelDetailDal = hotelDetailDal;
            _hotelDal = hotelDal;
        }

        [HttpGet]
        public IActionResult GetBookings()
        {
            var bookings = _bookingService.TGetList();
            return Ok(bookings);
        }

        // enhanced GET: returns booking + room details with basic room/hotel info
        [HttpGet("{id}")]
        public IActionResult GetBooking(int id)
        {
            var booking = _bookingService.TGetByID(id);
            if (booking == null)
                return NotFound();

            // load room-detail rows for this booking
            var rdList = _roomDetailDal.GetList().Where(x => x.BookingId == id).ToList();

            var roomDetailsDto = rdList.Select(rd =>
            {
                // room info
                var room = _roomDal.GetByID(rd.RoomId);
                string roomName = room?.RoomName ?? string.Empty;
                decimal roomPrice = room?.Price ?? 0m;

                // hotel detail -> hotel
                string hotelName = string.Empty;
                string hotelAddress = string.Empty;
                if (rd.HotelDetailId.HasValue && rd.HotelDetailId.Value > 0)
                {
                    var hd = _hotelDetailDal.GetByID(rd.HotelDetailId.Value);
                    if (hd != null)
                    {
                        var h = _hotelDal.GetByID(hd.HotelId);
                        if (h != null)
                        {
                            hotelName = h.HotelName ?? string.Empty;
                            hotelAddress = h.Address ?? string.Empty;
                        }
                    }
                }

                return new
                {
                    roomDetailId = rd.Id,
                    bookingId = rd.BookingId,
                    roomId = rd.RoomId,
                    roomName = roomName,
                    quantity = rd.Quantity,
                    adultAmount = rd.AdultAmount,
                    childrenAmount = rd.ChildrenAmount,
                    price = roomPrice,
                    hotelDetailId = rd.HotelDetailId,
                    hotelName = hotelName,
                    hotelAddress = hotelAddress
                };
            }).ToList();

            var result = new
            {
                booking = booking,
                roomDetails = roomDetailsDto
            };

            return Ok(result);
        }

        [HttpPut]
        public IActionResult UpdateBooking(Booking booking)
        {
            _bookingService.TUpdate(booking);
            return Ok();
        }

        [HttpPost]
        public IActionResult AddBooking(Booking booking)
        {
            _bookingService.TInsert(booking);
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBooking(int id)
        {
            var booking = _bookingService.TGetByID(id);
            if (booking == null)
                return NotFound();

            _bookingService.TDelete(booking);
            return NoContent();
        }
    }
}
