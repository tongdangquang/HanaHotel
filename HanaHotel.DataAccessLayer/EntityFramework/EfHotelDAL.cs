using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DataAccessLayer.Repositories;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DataAccessLayer.EntityFramework
{
	public class EfHotelDAL : GenericRepository<Hotel>, IHotelDal
    {
        public EfHotelDAL(DataContext dataContext) : base(dataContext)
		{

		}
	}
}
