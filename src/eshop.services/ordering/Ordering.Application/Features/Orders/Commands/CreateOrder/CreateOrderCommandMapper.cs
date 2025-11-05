using Ordering.Application.Features.Orders.Dtos;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;
using Ordering.Domain.ValueObjects.Types;

namespace Ordering.Application.Features.Orders.Commands.CreateOrder;

public static class CreateOrderCommandMapper
{
    /// <summary>
    /// Creates a new Order domain object from the provided CreateOrderDto.
    /// </summary>
    /// <param name="requestOrder">The data transfer object containing order details.</param>
    /// <returns>A new instance of the Order domain object.</returns>
    public static Order CreateNewOrderFromDto(CreateOrderDto requestOrder)
    {
        var shippingAddress = Address.Of(requestOrder.ShippingAddress.FirstName, requestOrder.ShippingAddress.LastName, requestOrder.ShippingAddress.EmailAddress,
            requestOrder.ShippingAddress.AddressLine, requestOrder.ShippingAddress.Country, requestOrder.ShippingAddress.State, requestOrder.ShippingAddress.ZipCode);
        var billingAddress = Address.Of(requestOrder.BillingAddress.FirstName, requestOrder.BillingAddress.LastName, requestOrder.BillingAddress.EmailAddress, requestOrder.BillingAddress.AddressLine,
            requestOrder.BillingAddress.Country, requestOrder.BillingAddress.State, requestOrder.BillingAddress.ZipCode);
        var payment = Payment.Of(requestOrder.Payment.CardName, requestOrder.Payment.CardNumber, requestOrder.Payment.Expiration, requestOrder.Payment.Cvv, requestOrder.Payment.PaymentMethod);
       
        var order = Order.Create(customerId: CustomerId.Of(requestOrder.CustomerId), orderName : OrderName.Of(requestOrder.OrderName), shippingAddress: shippingAddress, billingAddress: billingAddress, payment: payment);
       
        // Mettre ? jour le statut de la commande si sp?cifi?
        if (requestOrder.OrderStatus != order.OrderStatus)
        {
            order.SetOrderStatus(requestOrder.OrderStatus);
        }
       
        foreach (var orderItem in requestOrder.OrderItems)
        {
            order.AddOrderItem(ProductId.Of(orderItem.ProductId), orderItem.Quantity, orderItem.Price, orderItem.ProductName ?? string.Empty);
        }
       
        return order;
    }
}