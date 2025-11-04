using BuildingBlocks.CQRS;

namespace Ordering.Application.Features.Orders.Commands.GetOrdersByCustomerId;

public record GetOrdersByCustomerIdCommand(Guid CustomerId) : ICommand<GetOrdersByCustomerIdCommandResult>;

