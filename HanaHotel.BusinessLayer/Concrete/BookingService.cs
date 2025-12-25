using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.BusinessLayer.Concrete
{
    public class BookingService : IBookingService
    {
        private readonly IBookingDal _bookingDal;
        private readonly IRoomDal _roomDal;
        private readonly IRoomDetailDal _roomDetailDal;

        public BookingService(IBookingDal bookingDal, IRoomDal roomDal, IRoomDetailDal roomDetailDal)
        {
            _bookingDal = bookingDal;
            _roomDal = roomDal;
			_roomDetailDal = roomDetailDal;
		}

        public void TDelete(Booking entity)
        {
            _bookingDal.Delete(entity);
            var roomDetail = _roomDetailDal.GetList().Where(x => x.BookingId == entity.Id);
            foreach (var item in roomDetail)
				_roomDetailDal.Delete(item);
		}   

        public Booking TGetByID(int id)
        {
            return _bookingDal.GetByID(id);

        }

        public List<Booking> TGetList()
        {
            return _bookingDal.GetList();
        }

        public void TInsert(Booking entity)
        {
            _bookingDal.Insert(entity);
        }

        public void TUpdate(Booking entity)
        {
            _bookingDal.Update(entity);
        }
    }
}