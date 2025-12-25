using System.ComponentModel.DataAnnotations;

namespace HanaHotel.EntityLayer.Concrete
{
    public class Room
    {
        [Key]
        public int Id { get; set; }
        public string RoomName { get; set; }
        public string Description { get; set; }
        public double Size { get; set; }
		public decimal Price { get; set; }
        public int BedCount { get; set; }

        public ICollection<PromotionDetail> PromotionDetails { get; set; } = new List<PromotionDetail>();
        public ICollection<ServiceDetail> RoomServices { get; set; } = new List<ServiceDetail>();
    }

}


