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

                    <tr>
                        <td class=""email-header"">
                            <img src=""https://cdn.discordapp.com/attachments/1039545043419136111/1435582767978119260/eshop.png?ex=690c7e23&is=690b2ca3&hm=ca1500c77ea7b925c37ac640074ebf8341fb731dc1e01aa74fe0a8a4dfd33c85&"" />
                            <h1 style=""margin: 10px 0 0 0; font-size: 28px; font-weight: 700;"">Confirmation de commande</h1>
                        </td>
                    </tr>

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