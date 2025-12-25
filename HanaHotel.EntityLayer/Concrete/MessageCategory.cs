namespace HanaHotel.EntityLayer.Concrete
{
    public class MessageCategory
    {
        public int MessageCategoryId { get; set; }
        public string MessageCategoryName { get; set; }

        public ICollection<Contact>? Contacts { get; set; }

    }
}