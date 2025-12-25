using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace HanaHotel.EntityLayer.Concrete
{
	public class HotelDetail
	{
		[Key]
		public int Id { get; set; }

		public int HotelId { get; set; }

		public int RoomId { get; set; }

		public int RoomCount { get; set; }

		public RoomStatus Status { get; set; }

		[ForeignKey(nameof(HotelId))]
		public Hotel? Hotel { get; set; }

		[ForeignKey(nameof(RoomId))]
		public Room? Room { get; set; }
	}

	public enum RoomStatus
	{
		Available = 0,
		Reserved = 1,
		Occupied = 2,
		Maintenance = 3
	}
}
