namespace HanaHotel.WebUI.Models.Staff
{
    public class UpdateStaffViewModel
    {
        public int StaffId { get; set; }
        public required string Name { get; set; }
        public required string Title { get; set; }
        public required string Link1 { get; set; }
        public required string Link2 { get; set; }
        public required string Link3 { get; set; }
    }
}
