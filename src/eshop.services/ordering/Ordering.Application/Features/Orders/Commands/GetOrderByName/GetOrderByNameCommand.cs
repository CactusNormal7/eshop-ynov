using BuildingBlocks.CQRS;

namespace Ordering.Application.Features.Orders.Commands.GetOrderByName;

public record GetOrderByNameCommand(string Name) : ICommand<GetOrderByNameCommandResult>;

