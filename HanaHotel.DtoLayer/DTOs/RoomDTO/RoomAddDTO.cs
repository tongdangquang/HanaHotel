using HanaHotel.EntityLayer.Concrete;
using System.ComponentModel.DataAnnotations;

namespace HanaHotel.DtoLayer.DTOs.RoomDTO
{
	public class RoomAddDTO
	{
		public string RoomName { get; set; }
		public RoomStatus Status { get; set; }

		[Required(ErrorMessage = "Description is required.")]
		public string Description { get; set; }
		public double Size { get; set; }

		[Required(ErrorMessage = "Price is required.")]
		[Range(0, int.MaxValue, ErrorMessage = "Price must be a non-negative number.")]
		public double Price { get; set; }

		[Required(ErrorMessage = "Bed count is required.")]
		public string BedCount { get; set; }

		// New: optional list of image paths that frontend already saved to wwwroot
		public List<string>? ImagePaths { get; set; }
	}
}