using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DataAccessLayer.Repositories;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DataAccessLayer.EntityFramework
{
    public class EfPromotionDAL : GenericRepository<Promotion>, IPromotionDal
    {
        public EfPromotionDAL(DataContext dataContext) : base(dataContext)
        {
        }
    }
}