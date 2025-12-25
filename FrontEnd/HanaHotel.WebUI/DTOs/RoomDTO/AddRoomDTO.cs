using System.ComponentModel.DataAnnotations;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.WebUI.DTOs.RoomDTO
{
    public class AddRoomDTO
    {
        [Required]
        public required string RoomName { get; set; }

        public RoomStatus Status { get; set; } = RoomStatus.Available;

        [Required]
        public required string Description { get; set; }

        [Range(0, double.MaxValue)]
        public double Size { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double Price { get; set; }

        [Required]
        public required string BedCount { get; set; }

        // Image paths to add (e.g. "hotel-html-template/img/<filename>")
        public List<string>? ImagePaths { get; set; }
    }
}
