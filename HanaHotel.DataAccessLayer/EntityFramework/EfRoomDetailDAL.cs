using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DataAccessLayer.Repositories;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DataAccessLayer.EntityFramework
{
    public class EfRoomDetailDAL : GenericRepository<RoomDetail>, IRoomDetailDal
    {
        public EfRoomDetailDAL(DataContext dataContext) : base(dataContext)
        {
        }
    }
}