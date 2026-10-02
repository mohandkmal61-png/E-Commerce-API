using ECommerce.Domin.Entitie.Identitymodule;
using ECommerce.Persistence.Data.Dbcontexts;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Persistence.DataSeed
{
    public class IdetityDataInitilazer : DataInitilizer
    {
        public IdetityDataInitilazer(StoreDbcontext dbcontext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) 
            : base(dbcontext, userManager, roleManager)
        {
        } 
    }
}
