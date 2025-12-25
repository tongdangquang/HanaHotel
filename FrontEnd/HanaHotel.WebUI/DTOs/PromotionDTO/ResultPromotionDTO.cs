using HanaHotel.WebUI.DTOs.PromotionDetailDTO;

namespace HanaHotel.WebUI.DTOs.PromotionDTO
{
    public class ResultPromotionDTO
    {
        public int PromotionId { get; set; }
        public string PromotionName { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<ResultPromotionDetailDTO>? Details { get; set; }
    }
}
