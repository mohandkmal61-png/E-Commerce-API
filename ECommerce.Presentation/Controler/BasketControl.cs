using ECommerce.Presentation.Extensions;
using ECommerce.ServicesAbstraction;
using E_Commerse.Sharde.DTOS.BasketDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBasketById(string id)
        {
            var result = await _basketServise.GetBasketByIdAsync(id);
            return result.ToActionResult(this);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateOrUpdateBasket([FromBody] CustomerBasketDto basketDto)
        {
            if (string.IsNullOrWhiteSpace(basketDto.Id))
                return BadRequest("Basket id is required.");

            var result = await _basketServise.CreateOrUpdateBasketAsync(basketDto);
            return result.ToActionResult(this);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBasket(string id)
        {
            var result = await _basketServise.DeleteBasketAsync(id);
            return result.ToActionResult(this);
        }
    }
}
