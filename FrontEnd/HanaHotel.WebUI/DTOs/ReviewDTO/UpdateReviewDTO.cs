using System.ComponentModel.DataAnnotations;

namespace HanaHotel.WebUI.DTOs.ReviewDTO
{
    public class UpdateReviewDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public double RatingStars { get; set; }

        public string? Content { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoomId { get; set; }
    }
}
