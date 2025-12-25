using System.ComponentModel.DataAnnotations;

namespace HanaHotel.EntityLayer.Concrete
{
    public class Contact
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }

        public string Message { get; set; }

        public DateTime Date { get; set; }

        public string? ResponseMessage { get; set; }
        public DateTime? ResponseDate { get; set; }
        public bool IsReplied { get; set; }

        public MessageCategory? MessageCategory { get; set; }
        public int MessageCategoryId { get; set; }

    }

}