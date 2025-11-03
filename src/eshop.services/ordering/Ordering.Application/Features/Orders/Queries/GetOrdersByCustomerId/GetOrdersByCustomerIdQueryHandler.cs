using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Extensions;
using Ordering.Application.Features.Orders.Data;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Domain.ValueObjects;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByCustomerId;

/// <summary>
/// Handles the query to retrieve orders by customer ID.
/// </summary>
public class GetOrdersByCustomerIdQueryHandler(IOrderingDbContext orderingDbContext) 
    : IQueryHandler<GetOrdersByCustomerIdQuery, IEnumerable<OrderDto>>
{
    /// <summary>
    /// Handles the execution logic for retrieving orders by customer ID.
    /// </summary>
    /// <param name="request">The query containing the customer identifier.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A collection of <see cref="OrderDto"/> objects associated with the specified customer.</returns>
    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByCustomerIdQuery request, CancellationToken cancellationToken)
    {
        var customerId = CustomerId.Of(request.CustomerId);
        
        var orders = await orderingDbContext.Orders
            .Where(order => order.CustomerId == customerId)
            .Include(order => order.OrderItems)
            .ToListAsync(cancellationToken);
        
        return orders.ToOrderDtoList();
    }
}
