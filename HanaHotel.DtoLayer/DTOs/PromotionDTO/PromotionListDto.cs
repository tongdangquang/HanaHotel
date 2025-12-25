using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanaHotel.DtoLayer.DTOs.PromotionDTO
{
    public class PromotionListDto
    {
        public int PromotionId { get; set; }
        public string PromotionName { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<PromotionDetailListDto> Details { get; set; } = new();
    }
}
