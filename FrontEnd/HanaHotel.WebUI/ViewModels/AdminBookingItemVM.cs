namespace HanaHotel.WebUI.ViewModels
{
    public class AdminBookingItemVM
    {
        public int BookingId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int AdultCount { get; set; }
        public int ChildCount { get; set; }

        // original Room (may be count or title) — keep as fallback
        public string Room { get; set; }

        // Displayed names resolved from RoomId/ServiceId
        public string RoomDisplay { get; set; }
        public string ServiceDisplay { get; set; }

        public HanaHotel.EntityLayer.Concrete.BookingStatus Status { get; set; }
        public string SpecialRequest { get; set; }
    }
}