using Ordering.Domain.Enums;

namespace Ordering.Application.Features.Orders.Dtos;

/// <summary>
/// DTO pour la création d'une commande sans Id requis
/// </summary>
public record CreateOrderDto(
    Guid CustomerId,
    string OrderName,
    AddressDto ShippingAddress,
    AddressDto BillingAddress,
    PaymentDto Payment,
    OrderStatus OrderStatus,
    List<CreateOrderItemDto> OrderItems);
