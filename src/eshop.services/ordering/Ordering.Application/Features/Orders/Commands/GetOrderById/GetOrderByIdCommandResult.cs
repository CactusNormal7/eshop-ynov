using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Commands.GetOrderById;

public record GetOrderByIdCommandResult(OrderDto? Order);

