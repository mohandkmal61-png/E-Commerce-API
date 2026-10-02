using E_Commerse.Sharde.DTOS.OrderDtos;
using ECommerce.Presentation.Extensions;
using ECommerce.ServicesAbstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Presentation.Controler
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class OrderControl : ControllerBase
    {
        private readonly IOrderServise _orderServise;

        public OrderControl(IOrderServise orderServise)
        {
            _orderServise = orderServise;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _orderServise.CreateOrderAsync(request);
            return result.ToActionResult(this);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserOrders()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var result = await _orderServise.GetUserOrdersAsync(email ?? string.Empty);
            return result.ToActionResult(this);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var result = await _orderServise.GetOrderByIdAsync(id, email ?? string.Empty);
            return result.ToActionResult(this);
        }

        [HttpGet("delivery-methods")]
        public async Task<IActionResult> GetDeliveryMethods()
        {
            var result = await _orderServise.GetDeliveryMethodsAsync();
            return result.ToActionResult(this);
        }

    }
}
