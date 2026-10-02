using ECommerce.Domin.Entitie.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Persistence.Data.Confugrations.OrderConfig
{
    public class DeliveryMethodConfig : IEntityTypeConfiguration<DaliveryMethod>
    {
        public void Configure(EntityTypeBuilder<DaliveryMethod> builder)
        {
            builder.Property(x => x.Price)
                .HasPrecision(8, 2);
            builder.Property(x => x.ShortName)
                .HasMaxLength(50);
            builder.Property(x => x.Description)
                .HasMaxLength(100);
            builder.Property(x => x.DeliveryTime)
                .HasMaxLength(50);
        }
    }
}
