using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HanaHotel.EntityLayer.Concrete
{
	public class ServiceDetail
	{
		[Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
		public int RoomId { get; set; }
		public int ServiceId { get; set; }
		public Room Room { get; set; }
        public Service Service { get; set; }
    }
}
