using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Domin.Entitie.Identitymodule
{
    public class ApplicationUser : IdentityUser
    {
        public Address? Address { get; set; }
    }
}
