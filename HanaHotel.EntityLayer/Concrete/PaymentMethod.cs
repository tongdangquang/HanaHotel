using System.ComponentModel.DataAnnotations;

namespace HanaHotel.EntityLayer.Concrete
{
	public class PaymentMethod
	{
		[Key]
		public int Id { get; set; }
		public string PaymentMethodName { get; set; }
	}
}
