using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanaHotel.DtoLayer.DTOs.PromotionDTO
{
    public class PromotionDetailListDto
    {
        public int PromotionDetailId { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
    }

}
