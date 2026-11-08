using System.Security.Cryptography;
using System.Text;
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
        private const string BasketKeyPrefix = "basket:v2:";

        private const string GetUserBasketScript = """
            local owner = redis.call('HGET', KEYS[1], 'ownerUserId')
            if owner ~= ARGV[1] then return nil end
            redis.call('PEXPIRE', KEYS[1], ARGV[2])
            return {
                redis.call('HGET', KEYS[1], 'id'),
                redis.call('HGET', KEYS[1], 'version'),
                redis.call('HGET', KEYS[1], 'items')
            }
            """;

        private const string GetGuestBasketScript = """
            if redis.call('HGET', KEYS[1], 'id') ~= ARGV[1] then return nil end
            if redis.call('HGET', KEYS[1], 'guestCredentialHash') ~= ARGV[2] then return nil end
            if redis.call('HEXISTS', KEYS[1], 'ownerUserId') == 1 then return nil end
            redis.call('PEXPIRE', KEYS[1], ARGV[3])
            return {
                redis.call('HGET', KEYS[1], 'id'),
                redis.call('HGET', KEYS[1], 'version'),
                redis.call('HGET', KEYS[1], 'items')
            }
            """;

        private const string GetOrCreateUserBasketScript = """
            if redis.call('EXISTS', KEYS[1]) == 1 then
                if redis.call('HGET', KEYS[1], 'ownerUserId') ~= ARGV[1] then return nil end
                redis.call('PEXPIRE', KEYS[1], ARGV[5])
                return {
                    redis.call('HGET', KEYS[1], 'id'),
                    redis.call('HGET', KEYS[1], 'version'),
                    redis.call('HGET', KEYS[1], 'items')
                }
            end
            redis.call('HSET', KEYS[1],
                'id', ARGV[2],
                'ownerUserId', ARGV[1],
                'version', ARGV[3],
                'items', ARGV[4])
            redis.call('PEXPIRE', KEYS[1], ARGV[5])
            return { ARGV[2], ARGV[3], ARGV[4] }
            """;

        private const string CreateGuestBasketScript = """
            if redis.call('EXISTS', KEYS[1]) == 1 then return 0 end
            redis.call('HSET', KEYS[1],
                'id', ARGV[1],
                'guestCredentialHash', ARGV[2],
                'version', ARGV[3],
                'items', ARGV[4])
            redis.call('PEXPIRE', KEYS[1], ARGV[5])
            return 1
            """;

        private const string SaveUserBasketScript = """
            if redis.call('HGET', KEYS[1], 'ownerUserId') ~= ARGV[1] then return nil end
            if redis.call('HGET', KEYS[1], 'id') ~= ARGV[2] then return nil end
            if redis.call('HGET', KEYS[1], 'version') ~= ARGV[3] then return nil end
            redis.call('HSET', KEYS[1], 'items', ARGV[4], 'version', ARGV[5])
            redis.call('PEXPIRE', KEYS[1], ARGV[6])
            return ARGV[5]
            """;

        private const string SaveGuestBasketScript = """
            if redis.call('HGET', KEYS[1], 'id') ~= ARGV[1] then return nil end
            if redis.call('HGET', KEYS[1], 'guestCredentialHash') ~= ARGV[2] then return nil end
            if redis.call('HEXISTS', KEYS[1], 'ownerUserId') == 1 then return nil end
            if redis.call('HGET', KEYS[1], 'version') ~= ARGV[3] then return nil end
            redis.call('HSET', KEYS[1], 'items', ARGV[4], 'version', ARGV[5])
            redis.call('PEXPIRE', KEYS[1], ARGV[6])
            return ARGV[5]
            """;

        private const string DeleteUserBasketScript = """
            if redis.call('HGET', KEYS[1], 'ownerUserId') ~= ARGV[1] then return 0 end
            if redis.call('HGET', KEYS[1], 'id') ~= ARGV[2] then return 0 end
            if redis.call('HGET', KEYS[1], 'version') ~= ARGV[3] then return 0 end
            return redis.call('DEL', KEYS[1])
            """;

        private const string DeleteGuestBasketScript = """
            if redis.call('HGET', KEYS[1], 'id') ~= ARGV[1] then return 0 end
            if redis.call('HGET', KEYS[1], 'guestCredentialHash') ~= ARGV[2] then return 0 end
            if redis.call('HEXISTS', KEYS[1], 'ownerUserId') == 1 then return 0 end
            if redis.call('HGET', KEYS[1], 'version') ~= ARGV[3] then return 0 end
            return redis.call('DEL', KEYS[1])
            """;

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

        public async Task<CustomerBasket?> GetUserBasketAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var result = await _database.ScriptEvaluateAsync(
                GetUserBasketScript,
                [GetUserKey(userId)],
                [userId, BasketTtlMilliseconds]);

            return DeserializeBasket(result, userId, null);
        }

        public async Task<CustomerBasket?> GetGuestBasketAsync(string basketId, string credentialHash)
        {
            if (string.IsNullOrWhiteSpace(basketId) || string.IsNullOrWhiteSpace(credentialHash))
                return null;

            var result = await _database.ScriptEvaluateAsync(
                GetGuestBasketScript,
                [GetGuestKey(basketId)],
                [basketId, credentialHash, BasketTtlMilliseconds]);

            return DeserializeBasket(result, null, credentialHash);
        }

        public async Task<CustomerBasket?> GetOrCreateUserBasketAsync(string userId, CustomerBasket newBasket)
        {
            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(newBasket.Id) ||
                !string.Equals(newBasket.OwnerUserId, userId, StringComparison.Ordinal) ||
                newBasket.GuestCredentialHash is not null ||
                newBasket.Version != 0)
            {
                return null;
            }

            var result = await _database.ScriptEvaluateAsync(
                GetOrCreateUserBasketScript,
                [GetUserKey(userId)],
                [
                    userId,
                    newBasket.Id,
                    newBasket.Version,
                    SerializeItems(newBasket.Items),
                    BasketTtlMilliseconds
                ]);

            return DeserializeBasket(result, userId, null);
        }

        public async Task<bool> CreateGuestBasketAsync(CustomerBasket basket)
        {
            if (string.IsNullOrWhiteSpace(basket.Id) ||
                string.IsNullOrWhiteSpace(basket.GuestCredentialHash) ||
                basket.OwnerUserId is not null ||
                basket.Version != 0)
            {
                return false;
            }

            var result = await _database.ScriptEvaluateAsync(
                CreateGuestBasketScript,
                [GetGuestKey(basket.Id)],
                [
                    basket.Id,
                    basket.GuestCredentialHash,
                    basket.Version,
                    SerializeItems(basket.Items),
                    BasketTtlMilliseconds
                ]);

            return (int)result == 1;
        }

        public async Task<CustomerBasket?> SaveUserBasketAsync(
            string userId,
            string expectedBasketId,
            long expectedVersion,
            IReadOnlyList<BasketItem> items)
        {
            var nextVersion = checked(expectedVersion + 1);
            var result = await _database.ScriptEvaluateAsync(
                SaveUserBasketScript,
                [GetUserKey(userId)],
                [
                    userId,
                    expectedBasketId,
                    expectedVersion,
                    SerializeItems(items),
                    nextVersion,
                    BasketTtlMilliseconds
                ]);

            if (result.IsNull)
                return null;

            return new CustomerBasket
            {
                Id = expectedBasketId,
                OwnerUserId = userId,
                Version = nextVersion,
                Items = CloneItems(items)
            };
        }

        public async Task<CustomerBasket?> SaveGuestBasketAsync(
            string basketId,
            string credentialHash,
            long expectedVersion,
            IReadOnlyList<BasketItem> items)
        {
            var nextVersion = checked(expectedVersion + 1);
            var result = await _database.ScriptEvaluateAsync(
                SaveGuestBasketScript,
                [GetGuestKey(basketId)],
                [
                    basketId,
                    credentialHash,
                    expectedVersion,
                    SerializeItems(items),
                    nextVersion,
                    BasketTtlMilliseconds
                ]);

            if (result.IsNull)
                return null;

            return new CustomerBasket
            {
                Id = basketId,
                GuestCredentialHash = credentialHash,
                Version = nextVersion,
                Items = CloneItems(items)
            };
        }

        public async Task<bool> DeleteUserBasketAsync(
            string userId,
            string expectedBasketId,
            long expectedVersion)
        {
            var result = await _database.ScriptEvaluateAsync(
                DeleteUserBasketScript,
                [GetUserKey(userId)],
                [userId, expectedBasketId, expectedVersion]);

            return (int)result == 1;
        }

        public async Task<bool> DeleteGuestBasketAsync(
            string basketId,
            string credentialHash,
            long expectedVersion)
        {
            var result = await _database.ScriptEvaluateAsync(
                DeleteGuestBasketScript,
                [GetGuestKey(basketId)],
                [basketId, credentialHash, expectedVersion]);

            return (int)result == 1;
        }

        private static CustomerBasket? DeserializeBasket(
            RedisResult result,
            string? ownerUserId,
            string? guestCredentialHash)
        {
            if (result.IsNull)
                return null;

            var fields = (RedisResult[])result;
            if (fields.Length != 3)
                throw new InvalidOperationException("Redis returned an invalid basket record.");

            var basketId = (string?)fields[0]
                ?? throw new InvalidOperationException("Redis basket id is missing.");
            var versionText = (string?)fields[1]
                ?? throw new InvalidOperationException("Redis basket version is missing.");
            var itemsJson = (string?)fields[2]
                ?? throw new InvalidOperationException("Redis basket items are missing.");

            if (!long.TryParse(versionText, out var version))
                throw new InvalidOperationException("Redis basket version is invalid.");

            return new CustomerBasket
            {
                Id = basketId,
                OwnerUserId = ownerUserId,
                GuestCredentialHash = guestCredentialHash,
                Version = version,
                Items = JsonSerializer.Deserialize<List<BasketItem>>(itemsJson, JsonOptions) ?? []
            };
        }

        private static string GetUserKey(string userId)
        {
            var userHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userId)));
            return $"{BasketKeyPrefix}user:{userHash}";
        }

        private static string GetGuestKey(string basketId) => $"{BasketKeyPrefix}guest:{basketId}";

        private static long BasketTtlMilliseconds => checked((long)BasketConstants.TimeToLive.TotalMilliseconds);

        private static string SerializeItems(IEnumerable<BasketItem> items)
            => JsonSerializer.Serialize(items, JsonOptions);

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
