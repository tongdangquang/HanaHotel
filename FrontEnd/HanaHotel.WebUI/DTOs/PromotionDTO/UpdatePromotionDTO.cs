using HanaHotel.WebUI.DTOs.PromotionDetailDTO;

namespace HanaHotel.WebUI.DTOs.PromotionDTO
{
    public class UpdatePromotionDTO
    {
        public int PromotionId { get; set; }
        public string PromotionName { get; set; } = null!;
        public decimal DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<UpdatePromotionDetailDTO>? Details { get; set; }
    }
}
