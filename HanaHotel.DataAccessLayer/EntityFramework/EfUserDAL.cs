using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DataAccessLayer.Repositories;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DataAccessLayer.EntityFramework
{
    public class EfUserDAL : GenericRepository<User>, IUserDal
	{
        public EfUserDAL(DataContext dataContext) : base(dataContext)
        {
        }
    }
}