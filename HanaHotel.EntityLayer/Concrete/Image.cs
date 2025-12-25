using System.ComponentModel.DataAnnotations;

namespace HanaHotel.EntityLayer.Concrete
{
	public class Image
	{
		[Key]
		public int Id { get; set; }
		public string ImagePath { get; set; }
		public int? RoomId { get; set; }
		public int? ReviewId { get; set; }
	}
}
