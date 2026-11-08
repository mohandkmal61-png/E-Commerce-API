using E_Commerse.Sharde.CommenResult;
using E_Commerse.Sharde.DTOS.BasketDtos;

namespace ECommerce.ServicesAbstraction
{
    public interface IBasketServise
    {
        Task<Result<CustomerBasketDto>> GetUserBasketAsync(string userId);
        Task<Result<CustomerBasketDto>> GetGuestBasketAsync(string basketId, string guestSecret);
        Task<Result<CustomerBasketDto>> CreateGuestBasketAsync(string basketId, string guestSecret);
        Task<Result<CustomerBasketDto>> SaveUserBasketAsync(string userId, SaveBasketRequestDto request);
        Task<Result<CustomerBasketDto>> SaveGuestBasketAsync(string guestSecret, SaveBasketRequestDto request);
        Task<Result<CustomerBasketDto>> MergeGuestBasketAsync(string userId, MergeBasketRequestDto request);
        Task<Result> DeleteUserBasketAsync(string userId, string expectedBasketId, long expectedVersion);
        Task<Result> DeleteGuestBasketAsync(string basketId, string guestSecret, long expectedVersion);
    }
}
