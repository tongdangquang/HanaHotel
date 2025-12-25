using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanaHotel.DtoLayer.DTOs.PromotionDTO
{
    public class PromotionDetailCreateDto
    {
        public int RoomId { get; set; }  // Phòng áp dụng khuyến mãi
        public decimal DiscountPercent { get; set; }
    }
}
