using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DataAccessLayer.Repositories;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DataAccessLayer.EntityFramework
{
    public class EfRoomDAL : GenericRepository<Room>, IRoomDal
    {
        public EfRoomDAL(DataContext dataContext) : base(dataContext)
        {
        }
    }
}