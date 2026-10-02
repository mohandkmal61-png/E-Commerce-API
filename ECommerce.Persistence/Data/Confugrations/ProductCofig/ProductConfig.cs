using ECommerce.Domin.Entitie.ProductModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Persistence.Data.Confugrations.ProductCofig
{
    public class ProductConfig : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(X => X.Name)
                .HasMaxLength(100);

            builder.Property(X => X.Description)    
                .HasMaxLength(500);
            builder.Property(X=>X.PuctureUrl)
                .HasMaxLength(200); 
            builder.Property(X=>X.Price)
                .HasPrecision(18 , 2);
            
            builder.HasOne(X => X.ProductBrands)
                .WithMany()
                .HasForeignKey(X => X.BrandId);

            builder.HasOne(X => X.ProductTypes)
                .WithMany()
                .HasForeignKey(X => X.TypeId);

        }
    }
}
