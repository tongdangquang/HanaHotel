using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanaHotel.DtoLayer.DTOs.HotelDTO
{
	// Minimal DTOs: only properties from User, Room, HotelDetail, Hotel
	public class HotelDto
	{
		public int Id { get; set; }
		public string? HotelName { get; set; }
		public int Status { get; set; }
		public string? Description { get; set; }
		public float? Size { get; set; }
		public string? Address { get; set; }
		public string? PhoneNumber { get; set; }
		public int? EmployeeCount { get; set; }

		// Manager minimal
		public UserDto? Manager { get; set; }

		// Hotel details minimal
		public List<HotelDetailDto> HotelDetails { get; set; } = new();
	}

	public class HotelDetailDto
	{
		public int Id { get; set; }
		public int HotelId { get; set; }
		public int RoomId { get; set; }
		public int RoomCount { get; set; }

		// Optional nested room minimal
		public RoomDto? Room { get; set; }
	}

	public class RoomDto
	{
		public int Id { get; set; }
		public string? RoomName { get; set; }
		public decimal Price { get; set; }
		public int BedCount { get; set; }
		public int Status { get; set; }
	}

	public class UserDto
	{
		public int Id { get; set; }
		public string? Name { get; set; }
		public string? Email { get; set; }
		public string? PhoneNumber { get; set; }
	}
}
