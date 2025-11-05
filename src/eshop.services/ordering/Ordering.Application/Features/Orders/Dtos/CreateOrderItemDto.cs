namespace Ordering.Application.Features.Orders.Dtos;

/// <summary>
/// DTO pour la création d'un item de commande sans OrderId requis
/// </summary>
public record CreateOrderItemDto(
    Guid ProductId,
    string? ProductName,
    int Quantity,
    decimal Price);
