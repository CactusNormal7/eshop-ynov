using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Commands.GetOrderByName;

public record GetOrderByNameCommandResult(OrderDto? Order);

