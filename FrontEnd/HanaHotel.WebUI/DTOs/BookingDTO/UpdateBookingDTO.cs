using System;
using System.ComponentModel.DataAnnotations;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.WebUI.DTOs.BookingDTO
{
    public class UpdateBookingDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        // Allow updating status from admin UI
        public BookingStatus Status { get; set; }

        public string? AdditionalRequest { get; set; }

        // Keep RoomId/UserId if you allow changing relations (optional)
        public int? RoomId { get; set; }
        public int? UserId { get; set; }
    }
}
