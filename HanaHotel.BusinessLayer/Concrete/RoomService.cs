using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.BusinessLayer.Concrete
{
    public class RoomService : IRoomService
    {
        private readonly IRoomDal _roomDal;

        public RoomService(IRoomDal roomDal)
        {
            _roomDal = roomDal;
        }

        public void TDelete(Room entity)
        {
            _roomDal.Delete(entity);
        }

        public Room TGetByID(int id)
        {
            return _roomDal.GetByID(id);
        }

        public List<Room> TGetList()
        {
            return _roomDal.GetList();
        }

        public void TInsert(Room entity)
        {
            _roomDal.Insert(entity);
        }

        public void TUpdate(Room entity)
        {
            _roomDal.Update(entity);
        }
    }
}