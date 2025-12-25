using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.BusinessLayer.Concrete
{
	public class ServiceDetailService : IServiceDetailService
	{
		void IGenericService<ServiceDetail>.TDelete(ServiceDetail entity)
		{
			throw new NotImplementedException();
		}

		ServiceDetail IGenericService<ServiceDetail>.TGetByID(int id)
		{
			throw new NotImplementedException();
		}

		List<ServiceDetail> IGenericService<ServiceDetail>.TGetList()
		{
			throw new NotImplementedException();
		}

		void IGenericService<ServiceDetail>.TInsert(ServiceDetail entity)
		{
			throw new NotImplementedException();
		}

		void IGenericService<ServiceDetail>.TUpdate(ServiceDetail entity)
		{
			throw new NotImplementedException();
		}
	}
}
