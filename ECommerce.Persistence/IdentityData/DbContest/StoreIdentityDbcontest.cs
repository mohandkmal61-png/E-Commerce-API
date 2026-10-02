using ECommerce.Domin.Entitie.Identitymodule;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Persistence.IdentityData.DbContest
{
    public class StoreIdentityDbcontest : IdentityDbContext<ApplicationUser>
    {
        public StoreIdentityDbcontest(DbContextOptions<StoreIdentityDbcontest> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<Address>().ToTable("Addresses");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
 

        }
    }
}
