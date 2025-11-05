using BuildingBlocks.CQRS;

namespace Ordering.Application.Features.Orders.Commands.GetOrders;

public record GetOrdersCommand(int PageIndex, int PageSize) : ICommand<GetOrdersCommandResult>;