using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DataAccessLayer.Repositories;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DataAccessLayer.EntityFramework
{
    public class EfPaymentMethodDAL : GenericRepository<PaymentMethod>, IPaymentMethodDal
    {
        public EfPaymentMethodDAL(DataContext dataContext) : base(dataContext)
        {

        }
    }
}


