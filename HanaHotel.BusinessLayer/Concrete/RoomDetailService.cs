using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanaHotel.BusinessLayer.Concrete
{
	public class RoomDetailService : IRoomDetailService
	{
		void IGenericService<RoomDetail>.TDelete(RoomDetail entity)
		{
			throw new NotImplementedException();
		}

		RoomDetail IGenericService<RoomDetail>.TGetByID(int id)
		{
			throw new NotImplementedException();
		}

		List<RoomDetail> IGenericService<RoomDetail>.TGetList()
		{
			throw new NotImplementedException();
		}

		void IGenericService<RoomDetail>.TInsert(RoomDetail entity)
		{
			throw new NotImplementedException();
		}

		void IGenericService<RoomDetail>.TUpdate(RoomDetail entity)
		{
			throw new NotImplementedException();
		}
	}
}
