using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DataAccessLayer.Repositories;
using HanaHotel.EntityLayer.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanaHotel.DataAccessLayer.EntityFramework
{
	public class EfHotelDetailDAL : GenericRepository<HotelDetail>, IHotelDetailDal
	{
		public EfHotelDetailDAL(DataContext dataContext) : base(dataContext)
		{
		}
	}
}
