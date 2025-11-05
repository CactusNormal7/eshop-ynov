using Basket.API.Data.Repositories;
using Basket.API.Models;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.UpdateBasket;

public class UpdateBasketCommandHandler(IBasketRepository repository) : ICommandHandler<UpdateBasketCommand, UpdateBasketCommandResult>
{
    public async Task<UpdateBasketCommandResult> Handle(UpdateBasketCommand request,
        CancellationToken cancellationToken)
    {
        var existingCart = await repository.GetBasketByUserNameAsync(request.UserName, cancellationToken);
        
        var updatedItems = new List<ShoppingCartItem>(existingCart.Items);
        
        foreach (var newItem in request.Items)
        {
            var existingItemById = updatedItems.FirstOrDefault(x => x.ProductId == newItem.ProductId);
            var existingItemByName = updatedItems.FirstOrDefault(x => x.ProductName == newItem.ProductName && x.ProductId != newItem.ProductId);
            
            if (existingItemById != null)
            {
                existingItemById.Quantity += newItem.Quantity;
                existingItemById.Price = newItem.Price;
                existingItemById.ProductName = newItem.ProductName;
                existingItemById.Color = newItem.Color;
            }
            else if (existingItemByName != null)
            {
                existingItemByName.Quantity += newItem.Quantity;
            }
            else
            {
                updatedItems.Add(newItem);
            }
        }
        
        var updatedCart = new ShoppingCart(request.UserName)
        {
            Items = updatedItems
        };

        var basketCart = await repository.UpdateBasketAsync(updatedCart, cancellationToken)
            .ConfigureAwait(false);

        return new UpdateBasketCommandResult(true, basketCart.UserName);
    }
}
