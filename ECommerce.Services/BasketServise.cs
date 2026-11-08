using AutoMapper;
using E_Commerse.Sharde.CommenResult;
using E_Commerse.Sharde.DTOS.BasketDtos;
using ECommerce.Domin.Contracts;
using ECommerce.Domin.Entitie.BasketModule;
using ECommerce.ServicesAbstraction;
using System.Security.Cryptography;
using System.Text;

namespace ECommerce.Services
{
    public class BasketServise : IBasketServise
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;

        public BasketServise(IBasketRepository basketRepository, IMapper mapper)
        {
            _basketRepository = basketRepository;
            _mapper = mapper;
        }

        public async Task<Result<CustomerBasketDto>> GetUserBasketAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<CustomerBasketDto>.fail(Error.Validation("UserIdRequired", "User ID is required."));

            var basket = await _basketRepository.GetUserBasketAsync(userId);
            if (basket is null)
            {
                var newBasket = new CustomerBasket
                {
                    Id = Guid.NewGuid().ToString(),
                    OwnerUserId = userId,
                    Version = 0,
                    Items = []
                };
                basket = await _basketRepository.GetOrCreateUserBasketAsync(userId, newBasket);
            }

            if (basket is null)
                return Result<CustomerBasketDto>.fail(Error.Failure("BasketLoadFailed", "Could not load or create user basket."));

            return Result<CustomerBasketDto>.ok(_mapper.Map<CustomerBasketDto>(basket));
        }

        public async Task<Result<CustomerBasketDto>> GetGuestBasketAsync(string basketId, string guestSecret)
        {
            if (string.IsNullOrWhiteSpace(basketId))
                return Result<CustomerBasketDto>.fail(Error.Validation("BasketIdRequired", "Basket ID is required."));

            if (string.IsNullOrWhiteSpace(guestSecret))
                return Result<CustomerBasketDto>.fail(Error.Unauthorized("GuestSecretRequired", "Guest secret is required."));

            var credentialHash = HashGuestSecret(guestSecret);
            var basket = await _basketRepository.GetGuestBasketAsync(basketId, credentialHash);
            if (basket is null)
                return Result<CustomerBasketDto>.fail(Error.NotFound("BasketNotFound", $"Guest basket with id '{basketId}' was not found or secret is invalid."));

            return Result<CustomerBasketDto>.ok(_mapper.Map<CustomerBasketDto>(basket));
        }

        public async Task<Result<CustomerBasketDto>> CreateGuestBasketAsync(string basketId, string guestSecret)
        {
            if (string.IsNullOrWhiteSpace(basketId))
                return Result<CustomerBasketDto>.fail(Error.Validation("BasketIdRequired", "Basket ID is required."));

            if (string.IsNullOrWhiteSpace(guestSecret))
                return Result<CustomerBasketDto>.fail(Error.Unauthorized("GuestSecretRequired", "Guest secret is required."));

            var credentialHash = HashGuestSecret(guestSecret);
            var newBasket = new CustomerBasket
            {
                Id = basketId,
                GuestCredentialHash = credentialHash,
                Version = 0,
                Items = []
            };

            var created = await _basketRepository.CreateGuestBasketAsync(newBasket);
            if (!created)
            {
                var existing = await _basketRepository.GetGuestBasketAsync(basketId, credentialHash);
                if (existing is not null)
                    return Result<CustomerBasketDto>.ok(_mapper.Map<CustomerBasketDto>(existing));

                return Result<CustomerBasketDto>.fail(Error.Validation("BasketAlreadyExists", "A guest basket with this ID already exists with different credentials."));
            }

            return Result<CustomerBasketDto>.ok(_mapper.Map<CustomerBasketDto>(newBasket));
        }

        public async Task<Result<CustomerBasketDto>> SaveUserBasketAsync(string userId, SaveBasketRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<CustomerBasketDto>.fail(Error.Validation("UserIdRequired", "User ID is required."));

            if (string.IsNullOrWhiteSpace(request.Id))
                return Result<CustomerBasketDto>.fail(Error.Validation("BasketIdRequired", "Basket ID is required."));

            request.Items ??= [];
            var items = _mapper.Map<List<BasketItem>>(request.Items);

            var current = await _basketRepository.GetUserBasketAsync(userId);
            if (current is null)
            {
                var newBasket = new CustomerBasket
                {
                    Id = request.Id,
                    OwnerUserId = userId,
                    Version = 0,
                    Items = items
                };
                var created = await _basketRepository.GetOrCreateUserBasketAsync(userId, newBasket);
                if (created is not null)
                    return Result<CustomerBasketDto>.ok(_mapper.Map<CustomerBasketDto>(created));
            }

            var saved = await _basketRepository.SaveUserBasketAsync(userId, request.Id, request.ExpectedVersion, items);
            if (saved is null)
            {
                var fresh = await _basketRepository.GetUserBasketAsync(userId);
                if (fresh is not null && fresh.Version != request.ExpectedVersion)
                {
                    return Result<CustomerBasketDto>.fail(Error.Validation("BasketConflict", "The basket was modified by another operation. Please refresh."));
                }

                return Result<CustomerBasketDto>.fail(Error.Validation("BasketSaveFailed", "Could not save user basket."));
            }

            return Result<CustomerBasketDto>.ok(_mapper.Map<CustomerBasketDto>(saved));
        }

        public async Task<Result<CustomerBasketDto>> SaveGuestBasketAsync(string guestSecret, SaveBasketRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(guestSecret))
                return Result<CustomerBasketDto>.fail(Error.Unauthorized("GuestSecretRequired", "Guest secret is required."));

            if (string.IsNullOrWhiteSpace(request.Id))
                return Result<CustomerBasketDto>.fail(Error.Validation("BasketIdRequired", "Basket ID is required."));

            request.Items ??= [];
            var items = _mapper.Map<List<BasketItem>>(request.Items);
            var credentialHash = HashGuestSecret(guestSecret);

            if (request.ExpectedVersion == 0)
            {
                var newGuestBasket = new CustomerBasket
                {
                    Id = request.Id,
                    GuestCredentialHash = credentialHash,
                    Version = 0,
                    Items = items
                };

                var created = await _basketRepository.CreateGuestBasketAsync(newGuestBasket);
                if (created)
                {
                    return Result<CustomerBasketDto>.ok(_mapper.Map<CustomerBasketDto>(newGuestBasket));
                }
            }

            var saved = await _basketRepository.SaveGuestBasketAsync(request.Id, credentialHash, request.ExpectedVersion, items);
            if (saved is null)
            {
                var fresh = await _basketRepository.GetGuestBasketAsync(request.Id, credentialHash);
                if (fresh is null)
                {
                    return Result<CustomerBasketDto>.fail(Error.NotFound("BasketNotFound", "Guest basket was not found or secret is invalid."));
                }

                if (fresh.Version != request.ExpectedVersion)
                {
                    return Result<CustomerBasketDto>.fail(Error.Validation("BasketConflict", "The basket was modified by another operation. Please refresh."));
                }

                return Result<CustomerBasketDto>.fail(Error.Validation("BasketSaveFailed", "Could not save guest basket."));
            }

            return Result<CustomerBasketDto>.ok(_mapper.Map<CustomerBasketDto>(saved));
        }

        public async Task<Result<CustomerBasketDto>> MergeGuestBasketAsync(string userId, MergeBasketRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<CustomerBasketDto>.fail(Error.Validation("UserIdRequired", "User ID is required."));

            if (string.IsNullOrWhiteSpace(request.GuestBasketId) || string.IsNullOrWhiteSpace(request.GuestSecret))
                return Result<CustomerBasketDto>.fail(Error.Validation("GuestCredentialsRequired", "Guest basket ID and secret are required."));

            var credentialHash = HashGuestSecret(request.GuestSecret);

            // 1. Fetch guest basket
            var guestBasket = await _basketRepository.GetGuestBasketAsync(request.GuestBasketId, credentialHash);
            if (guestBasket is null || guestBasket.Items.Count == 0)
            {
                // Idempotent: already merged or empty! Return current user basket
                return await GetUserBasketAsync(userId);
            }

            // 2. Fetch or create user basket
            var userBasket = await _basketRepository.GetUserBasketAsync(userId);
            if (userBasket is null)
            {
                var newBasket = new CustomerBasket
                {
                    Id = Guid.NewGuid().ToString(),
                    OwnerUserId = userId,
                    Version = 0,
                    Items = []
                };
                userBasket = await _basketRepository.GetOrCreateUserBasketAsync(userId, newBasket);
            }

            if (userBasket is null)
                return Result<CustomerBasketDto>.fail(Error.Failure("UserBasketFailed", "Could not retrieve user basket for merge."));

            // 3. Compute merged items
            var mergedItems = userBasket.Items.Select(item => new BasketItem
            {
                Id = item.Id,
                ProductName = item.ProductName,
                PictureUrl = item.PictureUrl,
                Price = item.Price,
                Quantity = item.Quantity
            }).ToList();

            foreach (var guestItem in guestBasket.Items)
            {
                var existing = mergedItems.FirstOrDefault(x => x.Id == guestItem.Id);
                if (existing is not null)
                {
                    existing.Quantity += guestItem.Quantity;
                }
                else
                {
                    mergedItems.Add(new BasketItem
                    {
                        Id = guestItem.Id,
                        ProductName = guestItem.ProductName,
                        PictureUrl = guestItem.PictureUrl,
                        Price = guestItem.Price,
                        Quantity = guestItem.Quantity
                    });
                }
            }

            // 4. Save to user basket with expected version
            var savedUserBasket = await _basketRepository.SaveUserBasketAsync(userId, userBasket.Id, userBasket.Version, mergedItems);
            if (savedUserBasket is null)
            {
                return Result<CustomerBasketDto>.fail(Error.Validation("BasketConflict", "User basket was modified concurrently during merge. Please refresh and retry."));
            }

            // 5. Delete guest basket. If delete fails, clear the items so retries will never duplicate quantities.
            var deleted = await _basketRepository.DeleteGuestBasketAsync(request.GuestBasketId, credentialHash, guestBasket.Version);
            if (!deleted)
            {
                await _basketRepository.SaveGuestBasketAsync(request.GuestBasketId, credentialHash, guestBasket.Version, []);
            }

            return Result<CustomerBasketDto>.ok(_mapper.Map<CustomerBasketDto>(savedUserBasket));
        }

        public async Task<Result> DeleteUserBasketAsync(string userId, string expectedBasketId, long expectedVersion)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(expectedBasketId))
                return Result.fail(Error.Validation("InvalidParameters", "User ID and expected basket ID are required."));

            var deleted = await _basketRepository.DeleteUserBasketAsync(userId, expectedBasketId, expectedVersion);
            return deleted
                ? Result.ok()
                : Result.fail(Error.NotFound("BasketNotFound", "User basket was not found or version mismatch."));
        }

        public async Task<Result> DeleteGuestBasketAsync(string basketId, string guestSecret, long expectedVersion)
        {
            if (string.IsNullOrWhiteSpace(basketId) || string.IsNullOrWhiteSpace(guestSecret))
                return Result.fail(Error.Validation("InvalidParameters", "Basket ID and guest secret are required."));

            var credentialHash = HashGuestSecret(guestSecret);
            var deleted = await _basketRepository.DeleteGuestBasketAsync(basketId, credentialHash, expectedVersion);
            return deleted
                ? Result.ok()
                : Result.fail(Error.NotFound("BasketNotFound", "Guest basket was not found, invalid secret, or version mismatch."));
        }

        private static string HashGuestSecret(string guestSecret)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(guestSecret)));
        }
    }
}