using System;
using System.Linq;
using System.Threading.Tasks;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.EntityLayer.Concrete;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HanaHotel.WebUI.Controllers
{
	public class AdminReportController : Controller
	{
		private readonly DataContext _db;

		public AdminReportController(DataContext db)
		{
			_db = db;
		}

		public IActionResult Index()
		{
			return View();
		}

		// GET /AdminReport/Data?start=yyyy-MM-dd&end=yyyy-MM-dd
		[HttpGet]
		public async Task<IActionResult> Data(string start = null, string end = null)
		{
			try
			{
				// Default: B = today (UTC date), A = B - 7
				var endDate = !string.IsNullOrEmpty(end)
					? DateTime.Parse(end).Date
					: DateTime.UtcNow.Date;
				var startDate = !string.IsNullOrEmpty(start)
					? DateTime.Parse(start).Date
					: endDate.AddDays(-7);

				// include endDate full day: use < endDate.AddDays(1)
				var nextDay = endDate.AddDays(1);

				var bookings = _db.Bookings
								  .AsNoTracking()
								  .Where(b => b.BookingDate >= startDate && b.BookingDate < nextDay && b.Status != BookingStatus.Cancelled);

				// room counts (sum of RoomDetail.Quantity grouped by Room.RoomName)
				var roomQuery = from b in bookings
								join rd in _db.RoomDetails.AsNoTracking() on b.Id equals rd.BookingId
								join r in _db.Rooms.AsNoTracking() on rd.RoomId equals r.Id into rj
								from r in rj.DefaultIfEmpty()
								group rd by (r != null ? r.RoomName : null) into g
								select new
								{
									name = g.Key ?? "Không xác định",
									count = g.Sum(x => x.Quantity)
								};

				var roomTypes = await roomQuery
					.OrderByDescending(x => x.count)
					.Take(10)
					.ToListAsync();

				// hotel counts (sum of RoomDetail.Quantity grouped by Hotel.HotelName)
				var hotelQuery = from b in bookings
								 join rd in _db.RoomDetails.AsNoTracking() on b.Id equals rd.BookingId
								 join hd in _db.HotelDetails.AsNoTracking() on rd.RoomId equals hd.RoomId into hdj
								 from hd in hdj.DefaultIfEmpty()
								 join h in _db.Hotels.AsNoTracking() on (hd != null ? hd.HotelId : 0) equals h.Id into hj
								 from h in hj.DefaultIfEmpty()
								 group rd by (h != null ? h.HotelName : null) into g
								 select new
								 {
									 name = g.Key ?? "Không xác định",
									 count = g.Sum(x => x.Quantity)
								 };

				var hotels = await hotelQuery
					.OrderByDescending(x => x.count)
					.Take(10)
					.ToListAsync();

				// revenue by day (sum of Room.Price * RoomDetail.Quantity grouped by booking date)
				var revenueByDayQuery = from b in bookings
										join rd in _db.RoomDetails.AsNoTracking() on b.Id equals rd.BookingId
										join r in _db.Rooms.AsNoTracking() on rd.RoomId equals r.Id
										group new { b, rd, r } by new { b.BookingDate.Year, b.BookingDate.Month, b.BookingDate.Day } into g
										select new
										{
											year = g.Key.Year,
											month = g.Key.Month,
											day = g.Key.Day,
											total = g.Sum(x => x.rd.Quantity * x.r.Price)
										};

				var revenueByDayRaw = await revenueByDayQuery
					.OrderBy(x => x.year).ThenBy(x => x.month).ThenBy(x => x.day)
					.ToListAsync();

				var revenueByDay = revenueByDayRaw
					.Select(x => new { date = new DateTime(x.year, x.month, x.day).ToString("yyyy-MM-dd"), total = x.total })
					.ToList();

				// revenue by hotel (sum of price * qty grouped by hotel)
				var revenueByHotelQuery = from b in bookings
										  join rd in _db.RoomDetails.AsNoTracking() on b.Id equals rd.BookingId
										  join r in _db.Rooms.AsNoTracking() on rd.RoomId equals r.Id
										  join hd in _db.HotelDetails.AsNoTracking() on rd.RoomId equals hd.RoomId into hdj
										  from hd in hdj.DefaultIfEmpty()
										  join h in _db.Hotels.AsNoTracking() on (hd != null ? hd.HotelId : 0) equals h.Id into hj
										  from h in hj.DefaultIfEmpty()
										  group new { rd, r } by (h != null ? h.HotelName : null) into g
										  select new
										  {
											  name = g.Key ?? "Không xác định",
											  total = g.Sum(x => x.rd.Quantity * x.r.Price)
										  };

				var revenueByHotel = await revenueByHotelQuery
					.OrderByDescending(x => x.total)
					.Take(10)
					.ToListAsync();

				return Json(new { roomTypes, hotels, revenueByDay, revenueByHotel, start = startDate.ToString("yyyy-MM-dd"), end = endDate.ToString("yyyy-MM-dd") });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = "Lỗi khi lấy dữ liệu thống kê", detail = ex.Message });
			}
		}
	}
}
