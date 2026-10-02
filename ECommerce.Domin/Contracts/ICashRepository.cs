using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domin.Contracts
{
    public interface ICashRepository
    {
        Task<String?> GetAsync(string key);

        Task SetAsync(string Cachekey, string Cachevalue, TimeSpan TimeToLive);

    }
}
