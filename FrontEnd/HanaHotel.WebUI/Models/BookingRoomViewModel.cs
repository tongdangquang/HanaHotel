using System;
using System.Collections.Generic;

namespace HanaHotel.WebUI.Models
{
    public class BookingRoomViewModel
    {
        public int BookingId { get; set; }
		public string FullName { get; set; }
		public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;

        // New: hotel info to show on booking details page
        public string HotelName { get; set; } = string.Empty;
        public string HotelAddress { get; set; } = string.Empty;

        public DateTime BookingDate { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Quantity { get; set; }
        public int AdultAmount { get; set; }
        public int ChildrenAmount { get; set; }
        public double Subtotal { get; set; }           // subtotal for this row (roomPrice * qty * nights)
        public string Status { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AdditionalRequest { get; set; } = string.Empty;

        // include full booking's room details so "Chi ti?t" can show all rooms in the booking
        public List<RoomDetailDto> BookingRoomDetails { get; set; } = new();

        // New: payment fields
        public decimal PaidAmount { get; set; } = 0m;
        public decimal DueAmount { get; set; } = 0m;
    }

	public class RoomDetailDto
	{
		public int RoomId { get; set; }
		public string RoomName { get; set; } = string.Empty;
		public int Quantity { get; set; }
		public int AdultAmount { get; set; }
		public int ChildrenAmount { get; set; }
		public decimal Price { get; set; }

        // New: services & promotion info (optional)
        public List<string> Services { get; set; } = new();
        public string? PromotionName { get; set; }
        public double? PromotionDiscountPercent { get; set; }
        public decimal? PromotionDiscountAmount { get; set; }
	}
}