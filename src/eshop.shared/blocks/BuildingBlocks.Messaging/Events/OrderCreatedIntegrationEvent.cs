using System;
using BuildingBlocks.Messaging.Dtos;

namespace BuildingBlocks.Messaging.Events;

public record OrderCreatedIntegrationEvent : IntegrationEvent
{
    public OrderDto Order { get; init; } = null!;
    public string Html { get; init; } = string.Empty;

    public OrderCreatedIntegrationEvent() { }

    public OrderCreatedIntegrationEvent(OrderDto order, string html)
    {
        Order = order;
        Html = html;
    }
}

