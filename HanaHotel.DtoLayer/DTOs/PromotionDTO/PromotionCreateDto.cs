using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanaHotel.DtoLayer.DTOs.PromotionDTO
{
    public class PromotionCreateDto
    {
        public int? PromotionID { get; set; }
        public string PromotionName { get; set; } = null!;
        public decimal DiscountAmount { get; set; }   // số tiền giảm (dùng để tính %)
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<PromotionDetailCreateDto> Details { get; set; } = new();
        public List<int> RoomIds { get; set; } = new();

    }
}
