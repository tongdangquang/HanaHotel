using System.ComponentModel.DataAnnotations;

namespace HanaHotel.WebUI.DTOs.LoginDTO
{
    public class LoginUserDTO
    {
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
