using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Email.MQTT.Features.Email.EventHandlers;

/// <summary>
/// Handles the SendEmailEvent to send emails via MQTT.
/// </summary>
public class SendEmailEventHandler(ILogger<SendEmailEventHandler> logger) : IConsumer<SendEmailEvent>
{
    public async Task Consume(ConsumeContext<SendEmailEvent> context)
    {
        logger.LogInformation("Processing email event for: {ToEmail}", context.Message.ToEmail);

        try
        {
            logger.LogInformation(
                "Email Details - To: {To}, From: {From}, Subject: {Subject}, HTML Length: {Length}",
                context.Message.ToEmail,
                context.Message.FromEmail,
                context.Message.Subject,
                context.Message.HtmlContent.Length
            );

            using var client = new SmtpClient("sandbox.smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("c6417f6833e1ce", "3c23dc7eff9036"),
                EnableSsl = true
            };

            using var message = new MailMessage
            {
                From = new MailAddress(context.Message.FromEmail , "eShop Email Service"),
                Subject = context.Message.Subject,
                Body = context.Message.HtmlContent,
                IsBodyHtml = true
            };
            
            message.To.Add(context.Message.ToEmail);

            await client.SendMailAsync(message);

            logger.LogInformation("Email sent successfully to: {ToEmail}", context.Message.ToEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to: {ToEmail}", context.Message.ToEmail);
            throw;
        }
    }
}

