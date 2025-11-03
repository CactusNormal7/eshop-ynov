using System.Reflection;
using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.Models;

namespace Ordering.Application.Features.Orders.Commands.GetOrders;

/// <summary>
/// Handles the retrieval of orders with pagination support.
/// </summary>
public class GetOrdersCommandHandler(IOrderingDbContext orderingDbContext): ICommandHandler<GetOrdersCommand, GetOrdersCommandResult>
{
    private static readonly FieldInfo OrderItemsField = typeof(Order).GetField("_orderItems", BindingFlags.NonPublic | BindingFlags.Instance)!;

    /// <summary>
    /// Handles the operation for retrieving a paginated list of orders.
    /// </summary>
    /// <param name="request">The command containing pagination parameters (pageIndex and pageSize).</param>
    /// <param name="cancellationToken">Token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="GetOrdersCommandResult"/> containing the paginated list of orders.</returns>
    public async Task<GetOrdersCommandResult> Handle(GetOrdersCommand request, CancellationToken cancellationToken)
    {
        var orders = await orderingDbContext.Orders
            .OrderBy(o => EF.Property<Guid>(o, "Id"))
            // .Skip(request.PageIndex * request.PageSize)
            // .Take(request.PageSize)
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

        return new GetOrdersCommandResult(orderDtos);
    }

}