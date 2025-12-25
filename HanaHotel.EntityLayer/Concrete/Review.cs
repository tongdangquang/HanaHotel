using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HanaHotel.EntityLayer.Concrete
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        public double RatingStars { get; set; }

        [MaybeNull]
        public string Content { get; set; }
        public int UserId { get; set; }
        public int RoomId { get; set; }
    }
}