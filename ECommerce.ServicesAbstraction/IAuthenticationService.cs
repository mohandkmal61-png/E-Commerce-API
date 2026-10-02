using E_Commerse.Sharde.CommenResult;
using E_Commerse.Sharde.DTOS.AuthDtos;

namespace ECommerce.ServicesAbstraction
{
    public interface IAuthenticationService
    {
        Task<Result<UserResponse>> LoginAsync(LoginRequest request);
        Task<Result<UserResponse>> RegisterAsync(RegisterRequest request);
        Task<Result<bool>> EmailExistsAsync(string email);
        Task<Result<UserResponse>> GetCurrentUserAsync(string email);
    }
}
