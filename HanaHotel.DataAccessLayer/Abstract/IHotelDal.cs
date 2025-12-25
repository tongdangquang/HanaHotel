
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DataAccessLayer.Abstract
{
	public interface IHotelDal : IGenericDal<Hotel>
	{
		public void Delete(Hotel entity)
		{
			throw new NotImplementedException();
		}

		public Hotel GetByID(int id)
		{
			throw new NotImplementedException();
		}

		public List<Hotel> GetList()
		{
			throw new NotImplementedException();
		}

		public void Insert(Hotel entity)
		{
			throw new NotImplementedException();
		}

		public void Update(Hotel entity)
		{
			throw new NotImplementedException();
		}
	}
}
