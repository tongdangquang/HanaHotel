using System.ComponentModel.DataAnnotations;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DtoLayer.DTOs.RoomDTO
{
	public class UpdateRoomDTO
	{
		[Required]
		public int Id { get; set; }

		[Required]
		public required string RoomName { get; set; }

		[Required]
		public RoomStatus Status { get; set; }

		[Required]
		public required string Description { get; set; }

		[Range(0, double.MaxValue)]
		public double Size { get; set; }

		[Required]
		[Range(0, double.MaxValue)]
		public double Price { get; set; }

		[Required]
		public required string BedCount { get; set; }

		// New: image paths to add (these are saved on front-end's wwwroot)
		public List<string>? ImagePaths { get; set; }

		// New: ids of images to remove
		public int[]? RemoveImageIds { get; set; }

		// New: paths of images to remove (optional)
		public string[]? RemoveImagePaths { get; set; }
	}
}