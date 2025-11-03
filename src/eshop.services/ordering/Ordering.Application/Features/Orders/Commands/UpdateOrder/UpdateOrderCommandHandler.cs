using BuildingBlocks.CQRS;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.ValueObjects.Types;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrder;

/// <summary>
/// Handles the update order command, allowing the modification of an existing order in the system.
/// This handler retrieves the specified order, updates it with new values, and persists the changes
/// to the database. If the order does not exist, an exception is thrown.
/// </summary>
public class UpdateOrderCommandHandler(IOrderingDbContext orderingDbContext) : ICommandHandler<UpdateOrderCommand, UpdateOrderCommandResult>
{
    public async Task<UpdateOrderCommandResult> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderingDbContext.Orders.FindAsync(OrderId.Of(request.Order.Id), cancellationToken);
        if (order is null)
            throw new KeyNotFoundException($"Order with Id {request.Order.Id} was not found.");
        
        UpdateOrderCommandMapper.UpdateOrderWithNewValues(order, request.Order);


        foreach (var newOrderItem in request.Order.OrderItems)
        {
            var existingOrderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == ProductId.Of(newOrderItem.ProductId));
            if (existingOrderItem != null)
            {
                existingOrderItem.Quantity = newOrderItem.Quantity;
                existingOrderItem.Price = newOrderItem.Price;
            }
            else
            {
                order.AddOrderItem(
                    ProductId.Of(newOrderItem.ProductId),
                    newOrderItem.Quantity,
                    newOrderItem.Price
                );
            }

        }

        await orderingDbContext.SaveChangesAsync(cancellationToken);
        return new UpdateOrderCommandResult(true);
    }
}