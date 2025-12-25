using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanaHotel.EntityLayer.Concrete
{
    public class PromotionDetail
    {
        public int PromotionDetailID { get; set; }

        // FK tới Promotion
        public int PromotionID { get; set; }
        public Promotion Promotion { get; set; } = null!;

        // FK tới Room (lưu tên cột hiện tại là RomID để khớp DB)
        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;

        // tự tính
        public decimal DiscountPercent { get; set; }  // phần trăm giảm, ví dụ 20.50
    }

}
