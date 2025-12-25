using System.ComponentModel.DataAnnotations;
using HanaHotel.WebUI.DTOs.RoomDTO;

namespace HanaHotel.WebUI.DTOs.ServiceDTO
{
    public class UpdateServiceDTO
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Service name is required.")]
        [StringLength(100, ErrorMessage = "Service name must be at most 100 characters.")]
        public string ServiceName { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a non-negative number.")]
        public double Price { get; set; }

        [Required(ErrorMessage = "Unit is required.")]
        [StringLength(50, ErrorMessage = "Unit must be at most 50 characters.")]
        public string Unit { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }
        public List<int> RoomIds { get; set; } = new List<int>();
        //public List<RoomDTO> Rooms { get; set; } = new List<RoomDTO>();
    }
}
