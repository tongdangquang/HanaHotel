using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DataAccessLayer.Repositories;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DataAccessLayer.EntityFramework
{
    public class EfBookingDAL : GenericRepository<Booking>, IBookingDal
    {
        public EfBookingDAL(DataContext dataContext) : base(dataContext)
        {
        }
    }
}