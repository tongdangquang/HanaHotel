using System;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.WebUI.DTOs.BookingDTO
{
    public class ResultBookingDTO
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string? Phone { get; set; }

        public DateTime BookingDate { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public BookingStatus Status { get; set; }

        public string? AdditionalRequest { get; set; }

        public int RoomId { get; set; }

        public int UserId { get; set; }
    }
}
