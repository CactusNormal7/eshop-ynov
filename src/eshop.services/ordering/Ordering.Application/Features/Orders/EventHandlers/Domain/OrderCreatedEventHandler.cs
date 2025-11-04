using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Ordering.Application.Extensions;
using Ordering.Domain.Events;
using BuildingBlocks.Messaging.Events;
using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.EventHandlers.Domain;

/// <summary>
/// Handles the domain event for an order being created.
/// This handler is responsible for processing the <see cref="OrderCreatedEvent"/>
/// and publishing an integration event based on the order details.
/// </summary>
public class OrderCreatedEventHandler(IPublishEndpoint publishEndpoint, IFeatureManager featureManager, ILogger<OrderCreatedEventHandler> logger) : INotificationHandler<OrderCreatedEvent>
{
    /// <summary>
    /// Handles the domain event when a new order is created.
    /// </summary>
    /// <param name="notification">The <see cref="OrderCreatedEvent"/> containing details of the created order.</param>
    /// <param name="cancellationToken">A cancellation token to observe while performing the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain Event Handled: {DomainEvent}", notification.GetType().Name);

        if (await featureManager.IsEnabledAsync("OrderFulfilment"))
        {
            var orderDto = notification.Order.ToOrderDto();

            // Generate HTML summary for the order
            var html = GenerateOrderHtml(orderDto);

            // Publish simple email event
            var emailEvent = new SendEmailEvent(
                toEmail: orderDto.BillingAddress.EmailAddress,
                fromEmail: "noreply@eshop.com",
                subject: $"Order Confirmation - {orderDto.OrderName}",
                htmlContent: html
            );

            await publishEndpoint.Publish(emailEvent, cancellationToken);
        }
    }

    private static string GenerateOrderHtml(OrderDto order)
    {
        // Minimal, safe HTML summary. For production prefer a template engine or Razor.
        var itemsHtml = string.Join('\n', order.OrderItems.Select(oi =>
            $"<tr><td>{oi.ProductId}</td><td>{oi.Quantity}</td><td>{oi.Price:C}</td></tr>"));

        var html = $@"<html>
        <body>
            <h1>Order Summary</h1>
            <p>Order Id: {order.Id}</p>
            <p>Customer: {order.CustomerId}</p>
            <table border='1' cellpadding='4' cellspacing='0'>
                <thead><tr><th>ProductId</th><th>Qty</th><th>Price</th></tr></thead>
                <tbody>
                    {itemsHtml}
                </tbody>
            </table>
            <p>Total: {order.OrderItems.Sum(i => i.Price * i.Quantity):C}</p>
        </body>
        </html>";

        return html;
    }
}