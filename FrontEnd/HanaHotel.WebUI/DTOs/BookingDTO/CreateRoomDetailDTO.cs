using System.ComponentModel.DataAnnotations;

namespace HanaHotel.WebUI.DTOs.BookingDTO
{
    public class CreateRoomDetailDTO
    {
        [Required]
        public int RoomId { get; set; }

        [Range(0, 100)]
        public int Quantity { get; set; }

        [Range(0, 100)]
        public int AdultAmount { get; set; }

        [Range(0, 100)]
        public int ChildrenAmount { get; set; }

        // New: optional HotelDetailId to preserve which hotel-detail (hotel + room mapping) was chosen
        public int? HotelDetailId { get; set; }
    }
}