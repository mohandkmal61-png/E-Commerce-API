using E_Commerse.Sharde.CommenResult;
using E_Commerse.Sharde.DTOS.OrderDtos;
using ECommerce.Domin.Contracts;
using ECommerce.Domin.Entitie.BasketModule;
using ECommerce.Domin.Entitie.Orders;
using ECommerce.Domin.Entitie.ProductModule;
using ECommerce.ServicesAbstraction;
using Microsoft.Extensions.Configuration;
using Stripe;
using StoreProduct = ECommerce.Domin.Entitie.ProductModule.Product;

namespace ECommerce.Services
{
    public class OrderServise : IOrderServise
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public OrderServise(
            IBasketRepository basketRepository,
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<Result<OrderDto>> CreateOrderAsync(CreateOrderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.BasketId))
                return Result<OrderDto>.fail(Error.Validation("BasketIdRequired", "Basket id is required."));

            if (string.IsNullOrWhiteSpace(request.CustomerEmail))
                return Result<OrderDto>.fail(Error.Validation("CustomerEmailRequired", "Customer email is required."));

            if (request.DeliveryMethodId <= 0)
                return Result<OrderDto>.fail(Error.Validation("DeliveryMethodRequired", "Delivery method is required."));

            if (request.ShippingAddress is null)
                return Result<OrderDto>.fail(Error.Validation("AddressRequired", "Shipping address is required."));

            if (string.IsNullOrWhiteSpace(request.PaymentIntentId))
                return Result<OrderDto>.fail(
                    Error.Validation("PaymentIntentRequired", "A Stripe payment intent is required before creating the order."));

            var basket = await _basketRepository.GetBasketAsync(request.BasketId);
            if (basket is null)
                return Result<OrderDto>.fail(Error.NotFound("BasketNotFound", "Basket was not found."));

            if (basket.Items is null || basket.Items.Count == 0)
                return Result<OrderDto>.fail(Error.Validation("BasketEmpty", "Basket is empty."));

            var deliveryMethodRepository = _unitOfWork.GetRepository<DaliveryMethod, int>();
            var deliveryMethod = await deliveryMethodRepository.GetByIdAsync(request.DeliveryMethodId);
            if (deliveryMethod is null)
                return Result<OrderDto>.fail(Error.NotFound("DeliveryMethodNotFound", "Selected delivery method was not found."));

            var productRepository = _unitOfWork.GetRepository<StoreProduct, int>();
            var orderItems = new List<OrderItem>();
            var subtotal = 0m;

            foreach (var basketItem in basket.Items)
            {
                if (basketItem.Quantity <= 0)
                    return Result<OrderDto>.fail(Error.Validation("InvalidQuantity", $"Quantity for product {basketItem.ProductName} must be greater than zero."));

                var product = await productRepository.GetByIdAsync(basketItem.Id);
                if (product is null)
                    return Result<OrderDto>.fail(Error.NotFound("ProductNotFound", $"Product with id {basketItem.Id} was not found."));

                var orderItem = new OrderItem
                {
                    ItemOrdered = new ProductItemOrdered
                    {
                        ProductItemId = product.Id,
                        ProductName = product.Name,
                        PictureUrl = product.PuctureUrl
                    },
                    Price = product.Price,
                    Quantity = basketItem.Quantity,
                };

                orderItems.Add(orderItem);
                subtotal += product.Price * basketItem.Quantity;
            }

            var paymentIntent = await GetPaymentIntentAsync(request.PaymentIntentId);
            var total = subtotal + deliveryMethod.Price;
            var expectedAmount = ConvertToStripeAmount(total);
            var expectedCurrency = _configuration["Stripe:Currency"] ?? "usd";

            if (paymentIntent.Amount != expectedAmount ||
                !string.Equals(paymentIntent.Currency, expectedCurrency, StringComparison.OrdinalIgnoreCase))
            {
                return Result<OrderDto>.fail(
                    Error.Validation("PaymentAmountMismatch", "The payment amount does not match the order total."));
            }

            if (paymentIntent.Status is "succeeded" or "canceled")
            {
                return Result<OrderDto>.fail(
                    Error.Validation("InvalidPaymentIntent", "The payment intent cannot be used for this order."));
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserEmail = request.CustomerEmail,
                OrderDate = DateTimeOffset.UtcNow,
                Address = new OrdeeAddres
                {
                    FirstName = request.ShippingAddress.FirstName,
                    LastName = request.ShippingAddress.LastName,
                    City = request.ShippingAddress.City,
                    Country = request.ShippingAddress.Country,
                    Street = request.ShippingAddress.Street
                },
                DEliveryMethodId = deliveryMethod.Id,
                DeliveryMethod = deliveryMethod,
                Items = orderItems,
                SubTotal = subtotal,
                OrderStatus = OrderStatus_.pending,
                PaymentIntentId = request.PaymentIntentId,
                PaymentStatus = paymentIntent.Status
            };

            var orderRepository = _unitOfWork.GetRepository<Order, Guid>();
            await orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            await new PaymentIntentService().UpdateAsync(
                paymentIntent.Id,
                new PaymentIntentUpdateOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        ["orderId"] = order.Id.ToString(),
                        ["basketId"] = request.BasketId,
                        ["customerEmail"] = request.CustomerEmail
                    }
                });

            await _basketRepository.DeleteBasketAsync(request.BasketId);

            var response = new OrderDto
            {
                Id = order.Id,
                UserEmail = order.UserEmail,
                OrderStatus = order.OrderStatus.ToString(),
                PaymentStatus = order.PaymentStatus,
                OrderDate = order.OrderDate,
                SubTotal = order.SubTotal,
                Total = order.GetTotal(),
                DeliveryMethod = order.DeliveryMethod.ShortName,
                Address = new AddressDto
                {
                    FirstName = order.Address.FirstName,
                    LastName = order.Address.LastName,
                    City = order.Address.City,
                    Country = order.Address.Country,
                    Street = order.Address.Street
                },
                Items = order.Items.Select(item => new OrderItemDto
                {
                    ProductId = item.ItemOrdered.ProductItemId,
                    ProductName = item.ItemOrdered.ProductName,
                    PictureUrl = item.ItemOrdered.PictureUrl,
                    Price = item.Price,
                    Quantity = item.Quantity
                }).ToList()
            };

            return Result<OrderDto>.ok(response);
        }

        public async Task<Result<IEnumerable<OrderDto>>> GetUserOrdersAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result<IEnumerable<OrderDto>>.fail(
                    Error.Unauthorized("UserEmailMissing", "The authenticated user's email was not found."));

            var orders = await _unitOfWork.GetRepository<Order, Guid>().GetAllAsync();
            var userOrders = orders
                .Where(order => string.Equals(order.UserEmail, email, StringComparison.OrdinalIgnoreCase))
                .Select(MapOrder)
                .ToList();

            return Result<IEnumerable<OrderDto>>.ok(userOrders);
        }

        public async Task<Result<OrderDto>> GetOrderByIdAsync(Guid id, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result<OrderDto>.fail(
                    Error.Unauthorized("UserEmailMissing", "The authenticated user's email was not found."));

            var order = await _unitOfWork.GetRepository<Order, Guid>().GetByIdAsync(id);
            if (order is null || !string.Equals(order.UserEmail, email, StringComparison.OrdinalIgnoreCase))
                return Result<OrderDto>.fail(
                    Error.NotFound("OrderNotFound", "The order was not found for the authenticated user."));

            return Result<OrderDto>.ok(MapOrder(order));
        }

        public async Task<Result<IEnumerable<DeliveryMethodDto>>> GetDeliveryMethodsAsync()
        {
            var deliveryMethods = await _unitOfWork
                .GetRepository<DaliveryMethod, int>()
                .GetAllAsync();

            return Result<IEnumerable<DeliveryMethodDto>>.ok(
                deliveryMethods.Select(deliveryMethod => new DeliveryMethodDto
                {
                    Id = deliveryMethod.Id,
                    ShortName = deliveryMethod.ShortName,
                    Description = deliveryMethod.Description,
                    DeliveryTime = deliveryMethod.DeliveryTime,
                    Price = deliveryMethod.Price
                }).ToList());
        }

        public async Task<Result<PaymentIntentDto>> CreatePaymentIntentAsync(
            CreatePaymentIntentRequest request,
            string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result<PaymentIntentDto>.fail(
                    Error.Unauthorized("UserEmailMissing", "The authenticated user's email was not found."));

            if (string.IsNullOrWhiteSpace(request.BasketId))
                return Result<PaymentIntentDto>.fail(
                    Error.Validation("BasketIdRequired", "Basket id is required."));

            var basket = await _basketRepository.GetBasketAsync(request.BasketId);
            if (basket is null)
                return Result<PaymentIntentDto>.fail(
                    Error.NotFound("BasketNotFound", "Basket was not found."));

            if (basket.Items.Count == 0)
                return Result<PaymentIntentDto>.fail(
                    Error.Validation("BasketEmpty", "Basket is empty."));

            var deliveryMethod = await _unitOfWork
                .GetRepository<DaliveryMethod, int>()
                .GetByIdAsync(request.DeliveryMethodId);
            if (deliveryMethod is null)
                return Result<PaymentIntentDto>.fail(
                    Error.NotFound("DeliveryMethodNotFound", "Selected delivery method was not found."));

            var subtotal = 0m;
            var productRepository = _unitOfWork.GetRepository<StoreProduct, int>();
            foreach (var basketItem in basket.Items)
            {
                if (basketItem.Quantity <= 0)
                    return Result<PaymentIntentDto>.fail(
                        Error.Validation("InvalidQuantity", "Basket item quantity must be greater than zero."));

                var product = await productRepository.GetByIdAsync(basketItem.Id);
                if (product is null)
                    return Result<PaymentIntentDto>.fail(
                        Error.NotFound("ProductNotFound", $"Product with id {basketItem.Id} was not found."));

                subtotal += product.Price * basketItem.Quantity;
            }

            var secretKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException("Stripe:SecretKey is not configured.");

            StripeConfiguration.ApiKey = secretKey;
            var paymentIntent = await new PaymentIntentService().CreateAsync(
                new PaymentIntentCreateOptions
                {
                    Amount = ConvertToStripeAmount(subtotal + deliveryMethod.Price),
                    Currency = _configuration["Stripe:Currency"] ?? "usd",
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        ["basketId"] = request.BasketId,
                        ["customerEmail"] = email
                    }
                });

            return Result<PaymentIntentDto>.ok(new PaymentIntentDto
            {
                ClientSecret = paymentIntent.ClientSecret,
                PaymentIntentId = paymentIntent.Id,
                Amount = paymentIntent.Amount,
                Currency = paymentIntent.Currency
            });
        }

        private async Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
        {
            var secretKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException("Stripe:SecretKey is not configured.");

            StripeConfiguration.ApiKey = secretKey;
            try
            {
                return await new PaymentIntentService().GetAsync(paymentIntentId);
            }
            catch (StripeException ex)
            {
                throw new InvalidOperationException(
                    "The Stripe payment intent could not be retrieved.",
                    ex);
            }
        }

        private static long ConvertToStripeAmount(decimal amount)
            => checked((long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero));

        public async Task ProcessStripeWebhookAsync(string json, string signature)
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            if (string.IsNullOrWhiteSpace(webhookSecret))
                throw new InvalidOperationException("Stripe:WebhookSecret is not configured.");

            var stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);
            if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
                return;

            var orderRepository = _unitOfWork.GetRepository<Order, Guid>();
            var order = (await orderRepository.GetAllAsync())
                .FirstOrDefault(item => item.PaymentIntentId == paymentIntent.Id);
            if (order is null)
                return;

            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                order.PaymentStatus = paymentIntent.Status;
                order.OrderStatus = OrderStatus_.paymentRecived;
            }
            else if (stripeEvent.Type == "payment_intent.payment_failed")
            {
                order.PaymentStatus = paymentIntent.Status;
                order.OrderStatus = OrderStatus_.paymentFailed;
            }
            else
            {
                return;
            }

            orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();
        }

        private static OrderDto MapOrder(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserEmail = order.UserEmail,
                OrderStatus = order.OrderStatus.ToString(),
                PaymentStatus = order.PaymentStatus,
                OrderDate = order.OrderDate,
                SubTotal = order.SubTotal,
                Total = order.GetTotal(),
                DeliveryMethod = order.DeliveryMethod.ShortName,
                Address = new AddressDto
                {
                    FirstName = order.Address.FirstName,
                    LastName = order.Address.LastName,
                    City = order.Address.City,
                    Country = order.Address.Country,
                    Street = order.Address.Street
                },
                Items = order.Items.Select(item => new OrderItemDto
                {
                    ProductId = item.ItemOrdered.ProductItemId,
                    ProductName = item.ItemOrdered.ProductName,
                    PictureUrl = item.ItemOrdered.PictureUrl,
                    Price = item.Price,
                    Quantity = item.Quantity
                }).ToList()
            };
        }
    }
}
