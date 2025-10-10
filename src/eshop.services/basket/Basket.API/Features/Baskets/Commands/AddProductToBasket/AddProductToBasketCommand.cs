using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.AddProductToBasket;

public record AddProductToBasketCommand(
    string Name,
    string? UserName,
    decimal Price,
    string ImageFile,
    List<string> Categories,
    string Description,
    int Quantity,
    string Color
) : ICommand<AddProductToBasketCommandResult>;