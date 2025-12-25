using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HanaHotel.EntityLayer.Concrete
{
	public class RoomDetail
	{
		[Key]
		public int Id { get; set; }
		public int Quantity { get; set; }
		public int AdultAmount { get; set; }
		public int ChildrenAmount { get; set; }
		public int RoomId { get; set; }
		public int? HotelDetailId { get; set; }
		public int BookingId { get; set; }
	}
}
