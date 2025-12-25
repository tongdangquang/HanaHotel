
using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.BusinessLayer.Concrete
{
	public class PromotionDetailService : IPromotionDetailService
	{
		void IGenericService<PromotionDetail>.TDelete(PromotionDetail entity)
		{
			throw new NotImplementedException();
		}

		PromotionDetail IGenericService<PromotionDetail>.TGetByID(int id)
		{
			throw new NotImplementedException();
		}

		List<PromotionDetail> IGenericService<PromotionDetail>.TGetList()
		{
			throw new NotImplementedException();
		}

		void IGenericService<PromotionDetail>.TInsert(PromotionDetail entity)
		{
			throw new NotImplementedException();
		}

		void IGenericService<PromotionDetail>.TUpdate(PromotionDetail entity)
		{
			throw new NotImplementedException();
		}
	}
}
