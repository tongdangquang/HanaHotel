namespace HanaHotel.WebUI.DTOs.ReviewDTO
{
    public class ResultReviewDTO
    {
        public int Id { get; set; }
        public double RatingStars { get; set; }
        public string? Content { get; set; }
        public int UserId { get; set; }
        public int RoomId { get; set; }
    }
}
