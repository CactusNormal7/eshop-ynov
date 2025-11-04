using System.Reflection;
using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Features.Orders.Commands.GetOrders;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.Models;

namespace Ordering.Application.Features.Orders.Commands.GetOrdersByCustomerId;

/// <summary>
/// Handles the retrieval of orders filtered by customer ID.
/// </summary>
public class GetOrdersByCustomerIdCommandHandler(IOrderingDbContext orderingDbContext) : ICommandHandler<GetOrdersByCustomerIdCommand, GetOrdersByCustomerIdCommandResult>
{
    private static readonly FieldInfo OrderItemsField = typeof(Order).GetField("_orderItems", BindingFlags.NonPublic | BindingFlags.Instance)!;

    /// <summary>
    /// Handles the operation for retrieving orders by customer ID.
    /// </summary>
    /// <param name="request">The command containing the customer ID filter.</param>
    /// <param name="cancellationToken">Token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="GetOrdersByCustomerIdCommandResult"/> containing the filtered list of orders.</returns>
    public async Task<GetOrdersByCustomerIdCommandResult> Handle(GetOrdersByCustomerIdCommand request, CancellationToken cancellationToken)
    {
        var orders = await orderingDbContext.Orders
            .Where(o => EF.Property<Guid>(o, "CustomerId") == request.CustomerId)
            .OrderBy(o => EF.Property<Guid>(o, "Id"))
            .ToListAsync(cancellationToken);

        if (orders.Any())
        {
            var orderIds = orders.Select(o => o.Id.Value).ToList();
            var allOrderItems = await orderingDbContext.OrderItems
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var orderItems = allOrderItems
                .Where(oi => orderIds.Contains(oi.OrderId.Value))
                .ToList();

            foreach (var order in orders)
            {
                var items = orderItems.Where(oi => oi.OrderId.Value == order.Id.Value).ToList();
                if (OrderItemsField.GetValue(order) is List<OrderItem> orderItemsList)
                {
                    orderItemsList.Clear();
                    orderItemsList.AddRange(items);
                }
            }
        }

        var orderDtos = GetOrdersCommandMapper.MapToOrderDtoList(orders);

        return new GetOrdersByCustomerIdCommandResult(orderDtos);
    }
}

