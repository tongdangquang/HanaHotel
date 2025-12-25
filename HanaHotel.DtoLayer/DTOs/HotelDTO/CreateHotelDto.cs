using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanaHotel.DtoLayer.DTOs.HotelDTO
{
	public class CreateHotelDto
	{
		public string HotelName { get; set; } = string.Empty;
		public int Status { get; set; } = 1;
		public string? Description { get; set; }
		public float? Size { get; set; }
		public string? Address { get; set; }
		public string? PhoneNumber { get; set; }
		public int? EmployeeCount { get; set; }
	}
}
