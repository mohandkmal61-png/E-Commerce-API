using System;
using System.Collections.Generic;

namespace E_Commerse.Sharde.DTOS.OrderDtos
{
    public class CreateOrderRequest
    {
        public string BasketId { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public int DeliveryMethodId { get; set; }
        public AddressDto ShippingAddress { get; set; } = new();
        public string? PaymentIntentId { get; set; }
    }

    public class AddressDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
    }

    public class OrderDto
    {
        public Guid Id { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public string? PaymentStatus { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public AddressDto Address { get; set; } = new();
        public string DeliveryMethod { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string PictureUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class DeliveryMethodDto
    {
        public int Id { get; set; }
        public string ShortName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DeliveryTime { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class CreatePaymentIntentRequest
    {
        public string BasketId { get; set; } = string.Empty;
        public int DeliveryMethodId { get; set; }
    }

    public class PaymentIntentDto
    {
        public string ClientSecret { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}
