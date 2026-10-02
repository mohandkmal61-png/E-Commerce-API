using System.Collections.Concurrent;
using ECommerce.Domin.Contracts;
using ECommerce.Domin.Entitie.BasketModule;

namespace ECommerce.Persistence.Repositoris
{
    public class InMemoryBasketRepository : IBasketRepository
    {
        private readonly ConcurrentDictionary<string, CustomerBasket> _baskets = new();

        public Task<CustomerBasket?> GetBasketAsync(string basketId)
        {
            _baskets.TryGetValue(basketId, out var basket);
            return Task.FromResult<CustomerBasket?>(basket);
        }

        public Task<CustomerBasket?> CreateOrUpdateBasketAsync(CustomerBasket basket)
        {
            _baskets[basket.Id] = basket;
            return Task.FromResult<CustomerBasket?>(basket);
        }

        public Task<bool> DeleteBasketAsync(string basketId)
            => Task.FromResult(_baskets.TryRemove(basketId, out _));
    }
}
