using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.BusinessLayer.Concrete
{
	public class ReviewService : IReviewService
	{
		void IGenericService<Review>.TDelete(Review entity)
		{
			throw new NotImplementedException();
		}

		Review IGenericService<Review>.TGetByID(int id)
		{
			throw new NotImplementedException();
		}

		List<Review> IGenericService<Review>.TGetList()
		{
			throw new NotImplementedException();
		}

		void IGenericService<Review>.TInsert(Review entity)
		{
			throw new NotImplementedException();
		}

		void IGenericService<Review>.TUpdate(Review entity)
		{
			throw new NotImplementedException();
		}
	}
}
