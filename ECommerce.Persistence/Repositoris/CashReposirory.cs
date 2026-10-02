using ECommerce.Domin.Contracts;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Persistence.Repositoris
{
   

    public class CashReposirory : ICashRepository
    {
        private readonly IDatabase _database;

        public CashReposirory(IConnectionMultiplexer connection)
        {
            _database = connection.GetDatabase();
        }

        public async Task<string?> GetAsync(string key)
        {
            var cacheValue = await _database.StringGetAsync(key);

            if (cacheValue.IsNullOrEmpty)
                return null;

            return cacheValue;
        }

        public async Task SetAsync(string cacheKey, string cacheValue, TimeSpan timeToLive)
        {
            await _database.StringSetAsync(cacheKey, cacheValue, timeToLive);
        }
    }
}
