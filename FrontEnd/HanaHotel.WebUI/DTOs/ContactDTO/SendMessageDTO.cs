namespace HanaHotel.WebUI.DTOs.ContactDTO
{
    public class SendMessageDTO
    {
        public int ContactID { get; set; }
        public string Email { get; set; }
        public string? ResponseMessage { get; set; }
        public DateTime? ResponseDate { get; set; }
        public bool IsReplied { get; set; }
    }
}
