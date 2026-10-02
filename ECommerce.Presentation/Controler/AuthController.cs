using E_Commerse.Sharde.DTOS.AuthDtos;
using ECommerce.ServicesAbstraction;
using ECommerce.Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Presentation.Controler
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        /// <summary>
        /// User login endpoint - validates email and password, returns UserResponse with token
        /// </summary>
        /// <param name="request">LoginRequest containing email and password</param>
        /// <returns>UserResponse with email, display name, and JWT token on success; Unauthorized on failure</returns>
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authenticationService.LoginAsync(request);

            if (result.IsFailure)
                return Unauthorized(new { message = result.Errors.First().Description });

            return Ok(result.Value);
        }

        /// <summary>
        /// User registration endpoint - creates a new user account
        /// </summary>
        /// <param name="request">RegisterRequest containing email, username, password, display name, and phone number</param>
        /// <returns>UserResponse with email, display name, and JWT token on success; BadRequest with validation errors on failure</returns>
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authenticationService.RegisterAsync(request);

            if (result.IsFailure)
            {
                if (result.Errors.Any(e => e.Type == E_Commerse.Sharde.CommenResult.ErrorType.validatio))
                    return BadRequest(new { errors = result.Errors.Select(e => new { code = e.Code, description = e.Description }) });

                return StatusCode(500, new { message = "Registration failed", errors = result.Errors.Select(e => new { code = e.Code, description = e.Description }) });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Checks whether an account is registered for the specified email address.
        /// </summary>
        [HttpGet("email-exists")]
        public async Task<IActionResult> EmailExists([FromQuery] string email)
        {
            var result = await _authenticationService.EmailExistsAsync(email);
            return result.ToActionResult(this);
        }

        /// <summary>
        /// Retrieves the user associated with the specified email address and generates a token.
        /// </summary>
        [HttpGet("current-user")]
        public async Task<IActionResult> CurrentUser([FromQuery] string email)
        {
            var result = await _authenticationService.GetCurrentUserAsync(email);
            return result.ToActionResult(this);
        }
    }
}
