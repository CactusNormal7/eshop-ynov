using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Commands.GetOrders;

public record GetOrdersCommandResult(IEnumerable<OrderDto> Orders);