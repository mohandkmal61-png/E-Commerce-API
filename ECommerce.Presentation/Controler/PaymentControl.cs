using E_Commerse.Sharde.DTOS.OrderDtos;
using ECommerce.Presentation.Extensions;
using ECommerce.ServicesAbstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Presentation.Controler
{
    [ApiController]
    [Route("api/payment")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class PaymentControl : ControllerBase
    {
        private readonly IOrderServise _orderServise;

        public PaymentControl(IOrderServise orderServise)
        {
            _orderServise = orderServise;
        }

        [HttpPost("intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var result = await _orderServise.CreatePaymentIntentAsync(request, email ?? string.Empty);
            return result.ToActionResult(this);
        }

        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(signature))
                return BadRequest("Stripe-Signature header is required.");

            await _orderServise.ProcessStripeWebhookAsync(json, signature);
            return Ok();
        }
    }
}
