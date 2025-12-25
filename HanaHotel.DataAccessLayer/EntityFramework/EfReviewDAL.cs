using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DataAccessLayer.Repositories;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DataAccessLayer.EntityFramework
{
    public class EfReviewDAL : GenericRepository<Review>, IReviewDal
    {
        public EfReviewDAL(DataContext dataContext) : base(dataContext)
        {
        }
    }
}

