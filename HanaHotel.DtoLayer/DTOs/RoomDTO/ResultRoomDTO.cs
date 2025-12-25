using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DtoLayer.DTOs.RoomDTO
{
	public class ResultRoomDTO
	{
		public int Id { get; set; }
		public string RoomName { get; set; }
		public RoomStatus Status { get; set; }
		public string Description { get; set; }
		public double Size { get; set; }
		public decimal Price { get; set; }
		public string BedCount { get; set; }
		// trả về nhiều đường dẫn ảnh cho mỗi phòng
		public List<string>? ImagePaths { get; set; } = new List<string>();
	}
}
