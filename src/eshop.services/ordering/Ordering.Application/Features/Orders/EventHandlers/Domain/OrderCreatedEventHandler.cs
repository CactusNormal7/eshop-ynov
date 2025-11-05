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
public class OrderCreatedEventHandler(
    IPublishEndpoint publishEndpoint,
    IFeatureManager featureManager,
    ILogger<OrderCreatedEventHandler> logger) : INotificationHandler<OrderCreatedEvent>
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
            $@"<tr>
                <td style=""padding: 12px; border-bottom: 1px solid #e0e0e0;"">{oi.ProductId}</td>
                <td style=""padding: 12px; border-bottom: 1px solid #e0e0e0; text-align: center;"">{oi.Quantity}</td>
                <td style=""padding: 12px; border-bottom: 1px solid #e0e0e0; text-align: right; font-weight: bold;"">{oi.Price:C}</td>
            </tr>"));

        var total = order.OrderItems.Sum(i => i.Price * i.Quantity);

        var html = $@"<html lang=""fr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Confirmation de commande - eShop</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background-color: #f4f4f4;
            -webkit-font-smoothing: antialiased;
            -moz-osx-font-smoothing: grayscale;
        }}

        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}

        .email-header {{
            background: #FCB53B;
            color: #ffffff;
            padding: 30px 20px;
            text-align: center;
        }}

        .email-header img {{
            width: 120px;
            height: auto;
            margin-bottom: 10px;
        }}

        .email-body {{
            padding: 40px 30px;
            color: #333333;
            line-height: 1.6;
        }}

        .email-body h2 {{
            color: #2c3e50;
            margin-top: 0;
            margin-bottom: 20px;
            font-size: 24px;
            font-weight: 600;
        }}

        .email-body h3 {{
            color: #2c3e50;
            margin-top: 0;
            margin-bottom: 15px;
            font-size: 18px;
            font-weight: 600;
        }}

        .email-body p {{
            margin: 15px 0;
            font-size: 16px;
            color: #555555;
        }}

        .order-info {{
            background-color: #f8f9fa;
            padding: 20px;
            border-radius: 8px;
            margin: 25px 0;
        }}

        .order-info p {{
            margin: 8px 0;
            font-size: 15px;
        }}

        .order-info strong {{
            color: #2c3e50;
        }}

        .button-container {{
            text-align: center;
            margin: 30px 0;
        }}

        .button {{
            display: inline-block;
            padding: 14px 35px;
            background-color: #ff9d00;
            color: #ffffff !important;
            text-decoration: none;
            border-radius: 6px;
            font-weight: 600;
            font-size: 16px;
            transition: background-color 0.3s ease;
        }}

        .button:hover {{
            background-color: #e08a00;
        }}

        .products-table {{
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
            background-color: #ffffff;
        }}

        .products-table thead {{
            background-color: #f8f9fa;
        }}

        .products-table th {{
            padding: 15px 12px;
            text-align: left;
            font-weight: 600;
            color: #2c3e50;
            border-bottom: 2px solid #e0e0e0;
            font-size: 14px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }}

        .products-table th:last-child {{
            text-align: right;
        }}

        .products-table td {{
            padding: 12px;
            border-bottom: 1px solid #e0e0e0;
            font-size: 15px;
        }}

        .products-table tr:last-child td {{
            border-bottom: none;
        }}

        .total-section {{
            margin-top: 25px;
            padding-top: 20px;
            border-top: 2px solid #e0e0e0;
        }}

        .total-row {{
            padding: 15px 0;
            font-size: 18px;
            font-weight: 600;
            color: #2c3e50;
        }}

        .total-amount {{
            font-size: 24px;
            color: #ff9d00;
        }}

        .email-footer {{
            background-color: #2c3e50;
            padding: 25px 20px;
            text-align: center;
            color: #ffffff;
            font-size: 13px;
        }}

        .email-footer p {{
            margin: 8px 0;
            color: #ffffff;
        }}

        .email-footer a {{
            color: #ff9d00;
            text-decoration: none;
        }}

        .email-footer a:hover {{
            text-decoration: underline;
        }}

        .divider {{
            border-top: 1px solid #e0e0e0;
            margin: 30px 0;
        }}

        .greeting {{
            font-size: 18px;
            color: #2c3e50;
            margin-bottom: 20px;
        }}
    </style>
</head>

<body>
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f4f4f4; padding: 20px 0;"">
        <tr>
            <td align=""center"">
                <table class=""email-container"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #ffffff;"">
                    <!-- Header -->
                    <tr>
                        <td class=""email-header"">
                            <img src=""data:image/png;base64,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"" alt=""eShop Logo"">
                            <h1 style=""margin: 10px 0 0 0; font-size: 28px; font-weight: 700;"">Confirmation de commande</h1>
                        </td>
                    </tr>

                    <!-- Body -->
                    <tr>
                        <td class=""email-body"">
                            <div class=""greeting"">
                                Bonjour {order.BillingAddress.FirstName} {order.BillingAddress.LastName},
                            </div>

                            <p style=""font-size: 18px; color: #2c3e50; font-weight: 500;"">
                                Votre commande n°<strong>{order.Id}</strong> a été confirmée avec succès !
                            </p>

                            <div class=""order-info"">
                                <p><strong>Nom de la commande :</strong> {order.OrderName}</p>
                                <p><strong>Adresse de livraison :</strong></p>
                                <p style=""margin-left: 20px; margin-top: 5px;"">
                                    {order.BillingAddress.AddressLine}<br>
                                    {order.BillingAddress.Country}
                                </p>
                                <p style=""margin-top: 15px; color: #ff9d00; font-weight: 600;"">
                                    Livraison prévue sous 15 jours maximum
                                </p>
                            </div>

                            <div class=""divider""></div>

                            <h3>Détails de votre commande</h3>
                            
                            <table class=""products-table"">
                                <thead>
                                    <tr>
                                        <th>Produit</th>
                                        <th style=""text-align: center;"">Quantité</th>
                                        <th style=""text-align: right;"">Prix</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {itemsHtml}
                                </tbody>
                            </table>

                            <div class=""total-section"">
                                <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                                    <tr>
                                        <td class=""total-row"" style=""padding: 15px 0; font-size: 18px; font-weight: 600; color: #2c3e50;"">
                                            Total de la commande :
                                        </td>
                                        <td class=""total-row"" align=""right"" style=""padding: 15px 0; font-size: 24px; font-weight: 600; color: #ff9d00;"">
                                            {total:C}
                                        </td>
                                    </tr>
                                </table>
                            </div>

                            <div class=""button-container"">
                                <a href=""#"" class=""button"">Suivre ma commande</a>
                            </div>

                            <div class=""divider""></div>

                            <p style=""margin-top: 30px;"">
                                Si vous avez des questions concernant votre commande, n'hésitez pas à nous contacter.
                                Notre équipe est là pour vous aider !
                            </p>

                            <p style=""margin-top: 25px;"">
                                Cordialement,<br>
                                <strong style=""color: #2c3e50;"">L'équipe eShop Ynov</strong>
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td class=""email-footer"">
                            <p style=""font-size: 16px; font-weight: 600; margin-bottom: 15px;"">YNOV eShop</p>
                            <p>12 rue Georges Abitbol, 69005 Lyon, France</p>
                            <p style=""margin-top: 20px;"">
                                <a href=""#"">Se désabonner</a> | 
                                <a href=""#"">Mentions légales</a> | 
                                <a href=""#"">Contact</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

        return html;
    }
}