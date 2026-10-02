using ECommerce.Domin.Entitie.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Persistence.Data.Confugrations.OrderConfig
{
    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property(x => x.SubTotal)
                .HasPrecision(8, 2);
            builder.Property(x => x.PaymentIntentId)
                .HasMaxLength(100);
            builder.Property(x => x.PaymentStatus)
                .HasMaxLength(50);

            builder.OwnsOne(x => x.Address, addressBuilder =>
            {
                addressBuilder.Property(x => x.FirstName).HasMaxLength(50);
                addressBuilder.Property(x => x.LastName).HasMaxLength(50);
                addressBuilder.Property(x => x.City).HasMaxLength(50);
                addressBuilder.Property(x => x.Country).HasMaxLength(50);
                addressBuilder.Property(x => x.Street).HasMaxLength(50);
            });

            builder.HasOne(x => x.DeliveryMethod)
                .WithMany()
                .HasForeignKey(x => x.DEliveryMethodId);

            builder.OwnsMany(x => x.Items, itemBuilder =>
            {
                itemBuilder.WithOwner().HasForeignKey("OrderId");
                itemBuilder.Property(x => x.Price).HasPrecision(8, 2);
                itemBuilder.OwnsOne(x => x.ItemOrdered, productBuilder =>
                {
                    productBuilder.Property(x => x.ProductName).HasMaxLength(100);
                    productBuilder.Property(x => x.PictureUrl).HasMaxLength(200);
                });
            });
        }
    }
}
