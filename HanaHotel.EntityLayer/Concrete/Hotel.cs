using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HanaHotel.EntityLayer.Concrete
{
	public class Hotel
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string HotelName { get; set; } = null!;

		public HotelStatus Status { get; set; }

		public string? Description { get; set; }

		// diện tích/size — giữ double nếu bạn cần phép toán, hoặc chuyển sang decimal nếu cần chính xác hơn
		public double Size { get; set; }

		// FIX: address must be string, not decimal
		public string Address { get; set; } = string.Empty;

		public int EmployeeCount { get; set; }

		public string? PhoneNumber { get; set; }

		public int ManagerId { get; set; }

		[ForeignKey(nameof(ManagerId))]
		public User? Manager { get; set; }

		// Navigation: một Hotel có thể có nhiều HotelDetail (phòng / số lượng)
		public ICollection<HotelDetail> HotelDetails { get; set; } = new List<HotelDetail>();
	}

	public enum HotelStatus
	{
		Open = 0,
		Closed = 1,
		Maintenance = 2,
	}
}
