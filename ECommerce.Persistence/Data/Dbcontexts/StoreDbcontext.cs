using ECommerce.Domin.Entitie.ProductModule;
using ECommerce.Domin.Entitie.Orders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Persistence.Data.Dbcontexts
{
    public class StoreDbcontext : DbContext 
    {
        public StoreDbcontext(DbContextOptions<StoreDbcontext> options) : base(options)  
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductBrand> ProductBrands { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<DaliveryMethod> DeliveryMethods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StoreDbcontext).Assembly);  
        } 
    }
}
