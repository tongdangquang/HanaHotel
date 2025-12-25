using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanaHotel.EntityLayer.Concrete
{
    public class Promotion
    {
        public int PromotionID { get; set; }
        public string PromotionName { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; } // số tiền giảm (áp dụng cho từng phòng)
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<PromotionDetail> PromotionDetails { get; set; } = new List<PromotionDetail>();
    }
}
