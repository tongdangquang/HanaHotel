using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.BusinessLayer.Concrete
{
	public class UserService : IUserService
	{
		void IGenericService<User>.TDelete(User entity)
		{
			throw new NotImplementedException();
		}

		User IGenericService<User>.TGetByID(int id)
		{
			throw new NotImplementedException();
		}

		List<User> IGenericService<User>.TGetList()
		{
			throw new NotImplementedException();
		}

		void IGenericService<User>.TInsert(User entity)
		{
			throw new NotImplementedException();
		}

		void IGenericService<User>.TUpdate(User entity)
		{
			throw new NotImplementedException();
		}
	}
}
