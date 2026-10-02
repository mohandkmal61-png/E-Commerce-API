using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domin.Contracte
{
    public interface IDataInitilizer
    {
        Task InitilizeAsync();  
    }
}
