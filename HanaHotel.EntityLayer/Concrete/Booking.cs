using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HanaHotel.EntityLayer.Concrete
{
    public class Booking
    {
        [Key]
		public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime BookingDate { get; set; }
		public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public BookingStatus Status { get; set; }
        [MaybeNull]
		public string? AdditionalRequest { get; set; }
        public int RoomId { get; set; }
        public int UserId { get; set; }
    }

    public enum BookingStatus
    {
        Pending = 0,
		Deposited = 1,
		Paid = 2,
		Confirmed = 3,
        Cancelled = 4,
        Completed = 5
    }
}
