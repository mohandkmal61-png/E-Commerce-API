using E_Commerse.Sharde.CommenResult;
using E_Commerse.Sharde.DTOS.AuthDtos;
using ECommerce.Domin.Entitie.Identitymodule;
using ECommerce.Services.TokenService;
using ECommerce.ServicesAbstraction;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TokenService.TokenService _tokenService;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            TokenService.TokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<Result<UserResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                // Find user by email
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Result<UserResponse>.fail(
                        Error.Failure("InvalidEmail", "Email or password is incorrect"));
                }

                // Validate password
                var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!passwordValid)
                {
                    return Result<UserResponse>.fail(
                        Error.Failure("InvalidPassword", "Email or password is incorrect"));
                }

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);

                // Generate token
                var token = _tokenService.GenerateToken(user, roles);

                var response = new UserResponse
                {
                    Email = user.Email ?? string.Empty,
                    DisplayName = user.UserName ?? user.Email ?? "User",
                    Token = token
                };

                return Result<UserResponse>.ok(response);
            }
            catch (Exception ex)
            {
                return Result<UserResponse>.fail(
                    Error.Failure("LoginError", $"An error occurred during login: {ex.Message}"));
            }
        }

        public async Task<Result<UserResponse>> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return Result<UserResponse>.fail(
                        Error.Validation("EmailAlreadyExists", "Email is already registered"));
                }

                var userNameExists = await _userManager.FindByNameAsync(request.Username);
                if (userNameExists != null)
                {
                    return Result<UserResponse>.fail(
                        Error.Validation("UsernameAlreadyExists", "Username is already taken"));
                }

                // Create new user
                var newUser = new ApplicationUser
                {
                    UserName = request.Username,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(newUser, request.Password);
                if (!createResult.Succeeded)
                {
                    var errors = createResult.Errors
                        .Select(e => Error.Validation(e.Code, e.Description))
                        .ToList();

                    return Result<UserResponse>.fail(errors);
                }

                // Assign default User role
                await _userManager.AddToRoleAsync(newUser, "User");

                // Get user roles
                var roles = await _userManager.GetRolesAsync(newUser);

                // Generate token
                var token = _tokenService.GenerateToken(newUser, roles);

                var response = new UserResponse
                {
                    Email = newUser.Email ?? string.Empty,
                    DisplayName = newUser.UserName ?? newUser.Email ?? "User",
                    Token = token
                };

                return Result<UserResponse>.ok(response);
            }
            catch (Exception ex)
            {
                return Result<UserResponse>.fail(
                    Error.Failure("RegistrationError", $"An error occurred during registration: {ex.Message}"));
            }
        }

        public async Task<Result<bool>> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result<bool>.fail(
                    Error.Validation("EmailRequired", "Email is required"));
            }

            var user = await _userManager.FindByEmailAsync(email);
            return Result<bool>.ok(user is not null);
        }

        public async Task<Result<UserResponse>> GetCurrentUserAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result<UserResponse>.fail(
                    Error.Validation("EmailRequired", "Email is required"));
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return Result<UserResponse>.fail(
                    Error.NotFound("UserNotFound", "No user was found with the specified email address"));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var response = new UserResponse
            {
                Email = user.Email ?? string.Empty,
                DisplayName = user.UserName ?? user.Email ?? "User",
                Token = _tokenService.GenerateToken(user, roles)
            };

            return Result<UserResponse>.ok(response);
        }
    }
}
