using System;
using System.ComponentModel.DataAnnotations;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.WebUI.DTOs.UserDTO
{
    public class UpdateUserDTO
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên không được để trống")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public GenderType Gender { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Username không được để trống")]
        public string UserName { get; set; }

        // Optional: change password when provided
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{5,}$",
            ErrorMessage = "Mật khẩu phải có ít nhất 5 ký tự, 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
        public string? ConfirmPassword { get; set; }

        // RoleId kept because entity has it
        public int RoleId { get; set; }
    }
}
