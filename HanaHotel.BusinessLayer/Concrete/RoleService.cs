using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.BusinessLayer.Concrete
{
	public class RoleService : IRoleService
	{
		void IGenericService<Role>.TDelete(Role entity)
		{
			throw new NotImplementedException();
		}

		Role IGenericService<Role>.TGetByID(int id)
		{
			throw new NotImplementedException();
		}

		List<Role> IGenericService<Role>.TGetList()
		{
			throw new NotImplementedException();
		}

		void IGenericService<Role>.TInsert(Role entity)
		{
			throw new NotImplementedException();
		}

		void IGenericService<Role>.TUpdate(Role entity)
		{
			throw new NotImplementedException();
		}
	}
}
