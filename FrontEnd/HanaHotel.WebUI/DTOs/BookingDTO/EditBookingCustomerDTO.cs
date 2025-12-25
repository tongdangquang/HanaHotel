using System.ComponentModel.DataAnnotations;

namespace HanaHotel.WebUI.DTOs.BookingDTO
{
    public class EditBookingCustomerDTO
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        // Khách ch? ???c phép ch?nh yêu c?u ??c bi?t
        public string? AdditionalRequest { get; set; }
    }
}