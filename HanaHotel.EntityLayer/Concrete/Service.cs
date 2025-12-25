using System.ComponentModel.DataAnnotations;

namespace HanaHotel.EntityLayer.Concrete
{
    public class Service
    {
        [Key]
		public int Id { get; set; }
        public string ServiceName { get; set; }
        public double Price { get; set; }   
        public string Unit { get; set; }
        public string Description { get; set; }

        public string? ServiceIcon { get; set; }
        public ICollection<ServiceDetail> RoomServices { get; set; } = new List<ServiceDetail>();
    }
}
