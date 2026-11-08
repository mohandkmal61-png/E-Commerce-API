using System.Security.Cryptography;
using System.Text;
using ECommerce.Domin.Contracts;
using ECommerce.Domin.Entitie.BasketModule;

namespace ECommerce.Persistence.Repositoris
{
    public class InMemoryBasketRepository : IBasketRepository
    {
        private readonly object _gate = new();
        private readonly Dictionary<string, CustomerBasket> _baskets = new(StringComparer.Ordinal);

        public Task<CustomerBasket?> GetUserBasketAsync(string userId)
        {
            lock (_gate)
            {
                return Task.FromResult(
                    _baskets.TryGetValue(GetUserKey(userId), out var basket) &&
                    string.Equals(basket.OwnerUserId, userId, StringComparison.Ordinal)
                        ? Clone(basket)
                        : null);
            }
        }

        public Task<CustomerBasket?> GetGuestBasketAsync(string basketId, string credentialHash)
        {
            lock (_gate)
            {
                if (!_baskets.TryGetValue(GetGuestKey(basketId), out var basket) ||
                    basket.OwnerUserId is not null ||
                    !HashesMatch(basket.GuestCredentialHash, credentialHash))
                {
                    return Task.FromResult<CustomerBasket?>(null);
                }

                return Task.FromResult<CustomerBasket?>(Clone(basket));
            }
        }

        public Task<CustomerBasket?> GetOrCreateUserBasketAsync(string userId, CustomerBasket newBasket)
        {
            if (string.IsNullOrWhiteSpace(userId) ||
                !string.Equals(newBasket.OwnerUserId, userId, StringComparison.Ordinal) ||
                newBasket.GuestCredentialHash is not null ||
                newBasket.Version != 0 ||
                string.IsNullOrWhiteSpace(newBasket.Id))
            {
                return Task.FromResult<CustomerBasket?>(null);
            }

            lock (_gate)
            {
                var key = GetUserKey(userId);
                if (_baskets.TryGetValue(key, out var existing))
                {
                    return Task.FromResult<CustomerBasket?>(
                        string.Equals(existing.OwnerUserId, userId, StringComparison.Ordinal)
                            ? Clone(existing)
                            : null);
                }

                var basket = Clone(newBasket);
                _baskets.Add(key, basket);
                return Task.FromResult<CustomerBasket?>(Clone(basket));
            }
        }

        public Task<bool> CreateGuestBasketAsync(CustomerBasket basket)
        {
            if (string.IsNullOrWhiteSpace(basket.Id) ||
                basket.OwnerUserId is not null ||
                string.IsNullOrWhiteSpace(basket.GuestCredentialHash) ||
                basket.Version != 0)
            {
                return Task.FromResult(false);
            }

            lock (_gate)
            {
                return Task.FromResult(_baskets.TryAdd(GetGuestKey(basket.Id), Clone(basket)));
            }
        }

        public Task<CustomerBasket?> SaveUserBasketAsync(
            string userId,
            string expectedBasketId,
            long expectedVersion,
            IReadOnlyList<BasketItem> items)
        {
            var nextVersion = checked(expectedVersion + 1);
            lock (_gate)
            {
                var key = GetUserKey(userId);
                if (!_baskets.TryGetValue(key, out var current) ||
                    !string.Equals(current.OwnerUserId, userId, StringComparison.Ordinal) ||
                    !string.Equals(current.Id, expectedBasketId, StringComparison.Ordinal) ||
                    current.Version != expectedVersion)
                {
                    return Task.FromResult<CustomerBasket?>(null);
                }

                current.Items = CloneItems(items);
                current.Version = nextVersion;
                return Task.FromResult<CustomerBasket?>(Clone(current));
            }
        }

        public Task<CustomerBasket?> SaveGuestBasketAsync(
            string basketId,
            string credentialHash,
            long expectedVersion,
            IReadOnlyList<BasketItem> items)
        {
            var nextVersion = checked(expectedVersion + 1);
            lock (_gate)
            {
                var key = GetGuestKey(basketId);
                if (!_baskets.TryGetValue(key, out var current) ||
                    current.OwnerUserId is not null ||
                    !HashesMatch(current.GuestCredentialHash, credentialHash) ||
                    !string.Equals(current.Id, basketId, StringComparison.Ordinal) ||
                    current.Version != expectedVersion)
                {
                    return Task.FromResult<CustomerBasket?>(null);
                }

                current.Items = CloneItems(items);
                current.Version = nextVersion;
                return Task.FromResult<CustomerBasket?>(Clone(current));
            }
        }

        public Task<bool> DeleteUserBasketAsync(
            string userId,
            string expectedBasketId,
            long expectedVersion)
        {
            lock (_gate)
            {
                var key = GetUserKey(userId);
                return Task.FromResult(
                    _baskets.TryGetValue(key, out var current) &&
                    string.Equals(current.OwnerUserId, userId, StringComparison.Ordinal) &&
                    string.Equals(current.Id, expectedBasketId, StringComparison.Ordinal) &&
                    current.Version == expectedVersion &&
                    _baskets.Remove(key));
            }
        }

        public Task<bool> DeleteGuestBasketAsync(
            string basketId,
            string credentialHash,
            long expectedVersion)
        {
            lock (_gate)
            {
                var key = GetGuestKey(basketId);
                return Task.FromResult(
                    _baskets.TryGetValue(key, out var current) &&
                    current.OwnerUserId is null &&
                    HashesMatch(current.GuestCredentialHash, credentialHash) &&
                    string.Equals(current.Id, basketId, StringComparison.Ordinal) &&
                    current.Version == expectedVersion &&
                    _baskets.Remove(key));
            }
        }

        private static string GetUserKey(string userId) => $"user:{userId}";

        private static string GetGuestKey(string basketId) => $"guest:{basketId}";

        private static bool HashesMatch(string? storedHash, string providedHash)
        {
            if (storedHash is null || string.IsNullOrWhiteSpace(providedHash))
                return false;

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(storedHash),
                Encoding.UTF8.GetBytes(providedHash));
        }

        private static CustomerBasket Clone(CustomerBasket basket)
            => new()
            {
                Id = basket.Id,
                OwnerUserId = basket.OwnerUserId,
                GuestCredentialHash = basket.GuestCredentialHash,
                Version = basket.Version,
                Items = CloneItems(basket.Items)
            };

        private static List<BasketItem> CloneItems(IEnumerable<BasketItem> items)
            => items.Select(item => new BasketItem
            {
                Id = item.Id,
                ProductName = item.ProductName,
                PictureUrl = item.PictureUrl,
                Price = item.Price,
                Quantity = item.Quantity
            }).ToList();
    }
}
