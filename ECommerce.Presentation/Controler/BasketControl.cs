using ECommerce.Presentation.Extensions;
using ECommerce.ServicesAbstraction;
using E_Commerse.Sharde.DTOS.BasketDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Presentation.Controler
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class BasketControl : ControllerBase
    {
        private readonly IBasketServise _basketServise;

        public BasketControl(IBasketServise basketServise)
        {
            _basketServise = basketServise;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetBasket(
            [FromQuery] string? guestBasketId,
            [FromHeader(Name = "X-Basket-Guest-Secret")] string? guestSecret)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = GetAuthenticatedUserId();
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized("User identity could not be verified from token.");

                var result = await _basketServise.GetUserBasketAsync(userId);
                return result.ToActionResult(this);
            }

            if (string.IsNullOrWhiteSpace(guestBasketId) || string.IsNullOrWhiteSpace(guestSecret))
                return BadRequest("guestBasketId query parameter and X-Basket-Guest-Secret header are required for guest access.");

            var guestResult = await _basketServise.GetGuestBasketAsync(guestBasketId, guestSecret);
            return guestResult.ToActionResult(this);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBasketById(
            string id,
            [FromHeader(Name = "X-Basket-Guest-Secret")] string? guestSecret)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = GetAuthenticatedUserId();
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized("User identity could not be verified from token.");

                var result = await _basketServise.GetUserBasketAsync(userId);
                return result.ToActionResult(this);
            }

            if (string.IsNullOrWhiteSpace(guestSecret))
                return Unauthorized("X-Basket-Guest-Secret header is required to access guest basket.");

            var guestResult = await _basketServise.GetGuestBasketAsync(id, guestSecret);
            return guestResult.ToActionResult(this);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SaveBasket(
            [FromBody] SaveBasketRequestDto request,
            [FromHeader(Name = "X-Basket-Guest-Secret")] string? guestSecret)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
                return BadRequest("Basket id is required.");

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = GetAuthenticatedUserId();
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized("User identity could not be verified from token.");

                var result = await _basketServise.SaveUserBasketAsync(userId, request);
                return result.ToActionResult(this);
            }

            if (string.IsNullOrWhiteSpace(guestSecret))
                return Unauthorized("X-Basket-Guest-Secret header is required for guest basket updates.");

            var guestResult = await _basketServise.SaveGuestBasketAsync(guestSecret, request);
            return guestResult.ToActionResult(this);
        }

        [HttpPost("merge")]
        public async Task<IActionResult> MergeBasket([FromBody] MergeBasketRequestDto request)
        {
            var userId = GetAuthenticatedUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("User identity could not be verified from token.");

            if (string.IsNullOrWhiteSpace(request.GuestBasketId) || string.IsNullOrWhiteSpace(request.GuestSecret))
                return BadRequest("GuestBasketId and GuestSecret are required.");

            var result = await _basketServise.MergeGuestBasketAsync(userId, request);
            return result.ToActionResult(this);
        }

        [HttpDelete]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteBasket(
            [FromQuery] string basketId,
            [FromQuery] long version,
            [FromHeader(Name = "X-Basket-Guest-Secret")] string? guestSecret)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = GetAuthenticatedUserId();
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized("User identity could not be verified from token.");

                var result = await _basketServise.DeleteUserBasketAsync(userId, basketId, version);
                return result.ToActionResult(this);
            }

            if (string.IsNullOrWhiteSpace(guestSecret))
                return Unauthorized("X-Basket-Guest-Secret header is required to delete a guest basket.");

            var guestResult = await _basketServise.DeleteGuestBasketAsync(basketId, guestSecret, version);
            return guestResult.ToActionResult(this);
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteBasketById(
            string id,
            [FromQuery] long version,
            [FromHeader(Name = "X-Basket-Guest-Secret")] string? guestSecret)
        {
            return await DeleteBasket(id, version, guestSecret);
        }

        private string? GetAuthenticatedUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Email);
        }
    }
}