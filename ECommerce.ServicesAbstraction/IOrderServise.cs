using E_Commerse.Sharde.CommenResult;
using E_Commerse.Sharde.DTOS.OrderDtos;

namespace ECommerce.ServicesAbstraction
{
    public interface IOrderServise
    {
        Task<Result<OrderDto>> CreateOrderAsync(CreateOrderRequest request, string userEmail, string? userId = null);
        Task<Result<IEnumerable<OrderDto>>> GetUserOrdersAsync(string email);
        Task<Result<OrderDto>> GetOrderByIdAsync(Guid id, string email);
        Task<Result<IEnumerable<DeliveryMethodDto>>> GetDeliveryMethodsAsync();
        Task<Result<PaymentIntentDto>> CreatePaymentIntentAsync(CreatePaymentIntentRequest request, string email, string? userId = null);
        Task ProcessStripeWebhookAsync(string json, string signature);
    }
}
