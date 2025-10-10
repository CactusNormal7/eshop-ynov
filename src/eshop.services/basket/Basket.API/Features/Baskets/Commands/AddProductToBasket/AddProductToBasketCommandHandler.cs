using Basket.API.Data.Repositories;
using Basket.API.Models;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.AddProductToBasket;

/// <summary>
/// Handles the addition of a product to a shopping basket by processing the AddProductToBasketCommand.
/// Implements the <see cref="ICommandHandler{AddProductToBasketCommand, AddProductToBasketCommandResult}"/> interface.
/// </summary>
public class AddProductToBasketCommandHandler(IBasketRepository repository) : ICommandHandler<AddProductToBasketCommand, AddProductToBasketCommandResult>
{
    /// <summary>
    /// Handles the request to add a product to a shopping basket.
    /// </summary>
    /// <param name="request">The AddProductToBasketCommand containing the details of the product to be added to the basket.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task representing the asynchronous operation, returning an AddProductToBasketCommandResult that indicates the success of the operation and includes the UserName of the basket.</returns>
    public async Task<AddProductToBasketCommandResult> Handle(AddProductToBasketCommand request,
        CancellationToken cancellationToken)
    {
        // Get existing basket or create a new one
        var basket = await repository.GetBasketByUserNameAsync(request.UserName, cancellationToken);
        
        if (basket == null)
        {
            basket = new ShoppingCart(request.UserName);
        }

        // Create a new shopping cart item
        var cartItem = new ShoppingCartItem
        {
            ProductName = request.Name,
            Price = request.Price,
            Quantity = request.Quantity,
            Color = request.Color
        };

        // Add the item to the basket
        var itemsList = basket.Items.ToList();
        itemsList.Add(cartItem);
        basket.Items = itemsList;

        // Save the updated basket
        await repository.CreateBasketAsync(basket, cancellationToken);

        return new AddProductToBasketCommandResult(true, request.UserName);
    }
}