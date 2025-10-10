namespace Basket.API.Features.Baskets.Commands.AddProductToBasket;

/// <summary>
/// Represents the result of executing a command to add a product to a basket.
/// </summary>
public record AddProductToBasketCommandResult(bool IsSuccess, string UserName);