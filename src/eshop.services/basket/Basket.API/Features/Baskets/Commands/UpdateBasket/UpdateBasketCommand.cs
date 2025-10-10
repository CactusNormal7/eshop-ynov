using Basket.API.Models;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.UpdateBasket;

public record UpdateBasketCommand(string UserName, IEnumerable<ShoppingCartItem> Items) : ICommand<UpdateBasketCommandResult>;
