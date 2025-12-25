using System.ComponentModel.DataAnnotations;

namespace HanaHotel.WebUI.DTOs.ContactDTO
{
    public class CreateContactDTO
    {
        public int ContactID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Message is required")]
        public string Message { get; set; }

        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        public int MessageCategoryId { get; set; }
    }
}
