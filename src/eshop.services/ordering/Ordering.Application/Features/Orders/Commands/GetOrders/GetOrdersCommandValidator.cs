using FluentValidation;

namespace Ordering.Application.Features.Orders.Commands.GetOrders;

public class GetOrdersCommandValidator : AbstractValidator<GetOrdersCommand>
{
    public GetOrdersCommandValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Page index must be greater than or equal to 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0");
    }
}