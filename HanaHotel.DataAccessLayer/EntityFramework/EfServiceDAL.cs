using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DataAccessLayer.Repositories;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DataAccessLayer.EntityFramework
{
    public class EfServiceDAL : GenericRepository<Service>, IServiceDal
    {
        public EfServiceDAL(DataContext dataContext) : base(dataContext)
        {
        }
    }
}