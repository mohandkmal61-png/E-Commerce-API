using System.Text.Json;
using System.Text.Json.Serialization;
using E_Commerse.Sharde.Constants;
using ECommerce.Domin.Contracts;
using ECommerce.Domin.Entitie.BasketModule;
using StackExchange.Redis;

namespace ECommerce.Persistence.Repositoris
{
    public class RedisBasketRepository : IBasketRepository
    {
        private const string BasketKeyPrefix = "basket:";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly IDatabase _database;

        public RedisBasketRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task<CustomerBasket?> GetBasketAsync(string basketId)
        {
            var key = GetBasketKey(basketId);
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
                return null;

            await _database.KeyExpireAsync(key, BasketConstants.TimeToLive);

            return JsonSerializer.Deserialize<CustomerBasket>(value!, JsonOptions);
        }

        public async Task<CustomerBasket?> CreateOrUpdateBasketAsync(CustomerBasket basket)
        {
            var isSaved = await _database.StringSetAsync(
                GetBasketKey(basket.Id),
                Serialize(basket),
                BasketConstants.TimeToLive);

            return isSaved ? basket : null;
        }

        public async Task<bool> DeleteBasketAsync(string basketId)
        {
            return await _database.KeyDeleteAsync(GetBasketKey(basketId));
        }

        private static string GetBasketKey(string basketId) => $"{BasketKeyPrefix}{basketId}";

        private static string Serialize(CustomerBasket basket)
            => JsonSerializer.Serialize(basket, JsonOptions);
    }
}
