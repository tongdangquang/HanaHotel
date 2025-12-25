using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DataAccessLayer.Repositories;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DataAccessLayer.EntityFramework
{
    public class EfPromotionDetailDAL : GenericRepository<PromotionDetail>, IPromotionDetailDal
    {
        public EfPromotionDetailDAL(DataContext dataContext) : base(dataContext)
        {
        }
    }
}