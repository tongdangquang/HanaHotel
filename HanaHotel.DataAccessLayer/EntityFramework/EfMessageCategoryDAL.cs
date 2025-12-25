using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DataAccessLayer.Repositories;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DataAccessLayer.EntityFramework
{
    public class EfMessageCategoryDAL : GenericRepository<MessageCategory>, IMessageCategoryDal
    {
        public EfMessageCategoryDAL(DataContext dataContext) : base(dataContext)
        {
        }
    }
}