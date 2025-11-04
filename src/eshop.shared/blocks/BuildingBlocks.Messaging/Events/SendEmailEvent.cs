namespace BuildingBlocks.Messaging.Events;

/// <summary>
/// Simple email event containing only email addresses and HTML content.
/// </summary>
public record SendEmailEvent : IntegrationEvent
{
    public string ToEmail { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string HtmlContent { get; init; } = string.Empty;

    public SendEmailEvent() { }

    public SendEmailEvent(string toEmail, string fromEmail, string subject, string htmlContent)
    {
        ToEmail = toEmail;
        FromEmail = fromEmail;
        Subject = subject;
        HtmlContent = htmlContent;
    }
}

