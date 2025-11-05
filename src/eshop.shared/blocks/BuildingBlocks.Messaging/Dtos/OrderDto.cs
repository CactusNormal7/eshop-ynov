using BuildingBlocks.Messaging.Enums;
using Ordering.Application.Features.Orders.Dtos;

namespace BuildingBlocks.Messaging.Dtos;

public record OrderDto(Guid Id,
    Guid CustomerId, 
    String OrderName, 
    AddressDto ShippingAddress, 
    AddressDto  BillingAddress,
    PaymentDto Payment,
    OrderStatus OrderStatus,
    List<OrderItemDto> OrderItems);