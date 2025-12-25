using System.ComponentModel.DataAnnotations;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.WebUI.DTOs.RoomDTO
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

        // New: image paths to add (saved on server), will be sent to API
        public List<string>? ImagePaths { get; set; }

        // New: IDs of existing images to remove (bound from form checkboxes)
        public int[]? RemoveImageIds { get; set; }

        // New: paths of existing images to remove (if API uses paths)
        public string[]? RemoveImagePaths { get; set; }

        // Optional: API might return existing images as objects; view can still read via reflection
        // public List<ImageDto>? Images { get; set; } // optional depending on API
    }
}
