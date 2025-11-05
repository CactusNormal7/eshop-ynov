using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Commands.GetOrdersByCustomerId;

public record GetOrdersByCustomerIdCommandResult(IEnumerable<OrderDto> Orders);

