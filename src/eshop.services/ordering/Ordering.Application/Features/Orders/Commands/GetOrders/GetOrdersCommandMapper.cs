namespace Ordering.Application.Features.Orders.Commands.GetOrders;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Domain.Models;

public static class GetOrdersCommandMapper
{
    /// <summary>
    /// Maps an <see cref="Order"/> domain object to an <see cref="OrderDto"/> object.
    /// </summary>
    /// <param name="order">The <see cref="Order"/> instance to be mapped.</param>
    /// <returns>An instance of <see cref="OrderDto"/> that corresponds to the given <see cref="Order"/>.</returns>
    public static OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto(
            Id: order.Id.Value,
            CustomerId: order.CustomerId.Value,
            OrderName: order.OrderName.Value,
            ShippingAddress: new AddressDto(
                order.ShippingAddress.FirstName,
                order.ShippingAddress.LastName,
                order.ShippingAddress.EmailAddress!,
                order.ShippingAddress.AddressLine,
                order.ShippingAddress.Country,
                order.ShippingAddress.State,
                order.ShippingAddress.ZipCode),
            BillingAddress: new AddressDto(
                order.BillingAddress.FirstName,
                order.BillingAddress.LastName,
                order.BillingAddress.EmailAddress!,
                order.BillingAddress.AddressLine,
                order.BillingAddress.Country,
                order.BillingAddress.State,
                order.BillingAddress.ZipCode),
            Payment: new PaymentDto(
                order.Payment.CardName!,
                order.Payment.CardNumber,
                order.Payment.Expiration,
                order.Payment.CVV,
                order.Payment.PaymentMethod),
            OrderStatus: order.OrderStatus,
            OrderItems: order.OrderItems.Select(oi =>
                new OrderItemDto(oi.OrderId.Value, oi.ProductId.Value, oi.Quantity, oi.Price)).ToList()
        );
    }
    
    /// <summary>
    /// Maps a collection of <see cref="Order"/> domain objects to a collection of <see cref="OrderDto"/> objects.
    /// </summary>
    /// <param name="orders">The collection of <see cref="Order"/> instances to be mapped.</param>
    /// <returns>An enumerable collection of <see cref="OrderDto"/> objects.</returns>
    public static IEnumerable<OrderDto> MapToOrderDtoList(IEnumerable<Order> orders)
    {
        return orders.Select(MapToOrderDto);
    }
}