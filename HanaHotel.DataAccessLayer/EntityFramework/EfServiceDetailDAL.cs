using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DataAccessLayer.Repositories;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DataAccessLayer.EntityFramework
{
    public class EfServiceDetailDAL : GenericRepository<ServiceDetail>, IServiceDetailDal
	{
        public EfServiceDetailDAL(DataContext dataContext) : base(dataContext)
        {
        }
    }
}