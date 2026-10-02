using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domin.Entitie.Orders
{
    public class Order : BaseEntity<Guid>
    {
       public string UserEmail { get; set; } = null!;
       public DateTimeOffset OrderDate { get; set; }
       public OrdeeAddres Address { get; set; } = null!;
       public OrderStatus_ OrderStatus { get; set; } = OrderStatus_.pending;
       public string? PaymentIntentId { get; set; }
       public string? PaymentStatus { get; set; }

       [ForeignKey("DeliveryMethod")]
       public int DEliveryMethodId { get; set; }
       public DaliveryMethod DeliveryMethod { get; set; } = null!;

       public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

       public decimal SubTotal { get; set; }

       public decimal GetTotal()
           => SubTotal + DeliveryMethod.Price;
    }
}
