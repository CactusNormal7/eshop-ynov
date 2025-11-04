using BuildingBlocks.CQRS;

namespace Ordering.Application.Features.Orders.Commands.GetOrderById;

public record GetOrderByIdCommand(Guid OrderId) : ICommand<GetOrderByIdCommandResult>;

