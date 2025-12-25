using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.EntityLayer.Concrete;
using System.Collections.Generic;
using System.Linq;

namespace HanaHotel.BusinessLayer.Concrete
{
	public class HotelService : IHotelService
	{
		private readonly IHotelDal _hotelDal;
		private readonly IHotelDetailDal _hotelDetail;
		private readonly DataContext _db;

		public HotelService(IHotelDal hotelDal, IHotelDetailDal hotelDetail, DataContext db)
		{
			_hotelDal = hotelDal;
			_hotelDetail = hotelDetail;
			_db = db;
		}

		public void TDelete(Hotel entity)
		{
			_hotelDal.Delete(entity);
			var hotelDetails = _hotelDetail.GetList().Where(x => x.HotelId == entity.Id).ToList();
			foreach (var detail in hotelDetails)
			{
				_hotelDetail.Delete(detail);
			}
		}

		public Hotel TGetByID(int id)
		{
			// Lấy hotel cơ bản
			var hotel = _hotelDal.GetByID(id);
			if (hotel == null) return null;

			// Nạp manager (User) nếu có
			if (hotel.ManagerId != 0)
			{
				var manager = _db.Users.FirstOrDefault(u => u.Id == hotel.ManagerId);
				hotel.Manager = manager;
			}

			// Lấy tất cả HotelDetail cho hotel và nạp room cho từng detail
			var details = _hotelDetail.GetList().Where(d => d.HotelId == hotel.Id).ToList();
			if (details.Any())
			{
				var roomIds = details.Select(d => d.RoomId).Distinct().ToList();
				var rooms = _db.Rooms.Where(r => roomIds.Contains(r.Id)).ToList();

				foreach (var d in details)
				{
					d.Room = rooms.FirstOrDefault(r => r.Id == d.RoomId);
				}
				hotel.HotelDetails = details;
			}

			return hotel;
		}

		public List<Hotel> TGetList()
		{
			// Lấy danh sách hotels cơ bản từ DAL
			var hotels = _hotelDal.GetList() ?? new List<Hotel>();
			if (!hotels.Any()) return hotels;

			// Nạp tất cả managers dùng trong danh sách (minimize queries)
			var managerIds = hotels.Select(h => h.ManagerId).Where(id => id != 0).Distinct().ToList();
			var managers = managerIds.Any() ? _db.Users.Where(u => managerIds.Contains(u.Id)).ToList() : new List<User>();

			// Nạp tất cả hotelDetails cho các hotels
			var hotelIds = hotels.Select(h => h.Id).ToList();
			var allDetails = _hotelDetail.GetList().Where(d => hotelIds.Contains(d.HotelId)).ToList();

			// Nạp tất cả rooms được tham chiếu
			var roomIds = allDetails.Select(d => d.RoomId).Distinct().ToList();
			var rooms = roomIds.Any() ? _db.Rooms.Where(r => roomIds.Contains(r.Id)).ToList() : new List<Room>();

			// Gán manager, details và room cho từng hotel
			foreach (var h in hotels)
			{
				h.Manager = managers.FirstOrDefault(m => m.Id == h.ManagerId);

				var detailsForHotel = allDetails.Where(d => d.HotelId == h.Id).ToList();
				foreach (var d in detailsForHotel)
				{
					d.Room = rooms.FirstOrDefault(r => r.Id == d.RoomId);
				}
				h.HotelDetails = detailsForHotel;
			}

			// Optional: order by name for consistent output
			return hotels.OrderBy(h => h.HotelName).ToList();
		}

		public void TInsert(Hotel entity)
		{
			_hotelDal.Insert(entity);
		}

		public void TUpdate(Hotel entity)
		{
			_hotelDal.Update(entity);
		}
	}
}
