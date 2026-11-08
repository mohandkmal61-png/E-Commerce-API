using ECommerce.Domin.Entitie.BasketModule;

namespace ECommerce.Domin.Contracts
{
    public interface IBasketRepository
    {
        Task<CustomerBasket?> GetUserBasketAsync(string userId);
        Task<CustomerBasket?> GetGuestBasketAsync(string basketId, string credentialHash);
        Task<CustomerBasket?> GetOrCreateUserBasketAsync(string userId, CustomerBasket newBasket);
        Task<bool> CreateGuestBasketAsync(CustomerBasket basket);
        Task<CustomerBasket?> SaveUserBasketAsync(
            string userId,
            string expectedBasketId,
            long expectedVersion,
            IReadOnlyList<BasketItem> items);
        Task<CustomerBasket?> SaveGuestBasketAsync(
            string basketId,
            string credentialHash,
            long expectedVersion,
            IReadOnlyList<BasketItem> items);
        Task<bool> DeleteUserBasketAsync(string userId, string expectedBasketId, long expectedVersion);
        Task<bool> DeleteGuestBasketAsync(string basketId, string credentialHash, long expectedVersion);
    }
}
