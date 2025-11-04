using System.Reflection;
using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Features.Orders.Commands.GetOrders;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.Models;

namespace Ordering.Application.Features.Orders.Commands.GetOrderById;

/// <summary>
/// Handles the retrieval of an order by its unique identifier.
/// </summary>
public class GetOrderByIdCommandHandler(IOrderingDbContext orderingDbContext) : ICommandHandler<GetOrderByIdCommand, GetOrderByIdCommandResult>
{
    private static readonly FieldInfo OrderItemsField = typeof(Order).GetField("_orderItems", BindingFlags.NonPublic | BindingFlags.Instance)!;

    /// <summary>
    /// Handles the operation for retrieving an order by ID.
    /// </summary>
    /// <param name="request">The command containing the order ID.</param>
    /// <param name="cancellationToken">Token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="GetOrderByIdCommandResult"/> containing the order if found, null otherwise.</returns>
    public async Task<GetOrderByIdCommandResult> Handle(GetOrderByIdCommand request, CancellationToken cancellationToken)
    {
        var orders = await orderingDbContext.Orders
            .ToListAsync(cancellationToken);
        
        var order = orders.FirstOrDefault(o => o.Id.Value == request.OrderId);

        if (order == null)
        {
            return new GetOrderByIdCommandResult(null);
        }

        var allOrderItems = await orderingDbContext.OrderItems
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var orderItems = allOrderItems
            .Where(oi => oi.OrderId.Value == order.Id.Value)
            .ToList();

        if (OrderItemsField.GetValue(order) is List<OrderItem> orderItemsList)
        {
            orderItemsList.Clear();
            orderItemsList.AddRange(orderItems);
        }

        var orderDto = GetOrdersCommandMapper.MapToOrderDto(order);

        return new GetOrderByIdCommandResult(orderDto);
    }
}

