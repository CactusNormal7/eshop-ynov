using FluentValidation;

namespace Ordering.Application.Features.Orders.Commands.GetOrderById;

public class GetOrderByIdCommandValidator : AbstractValidator<GetOrderByIdCommand>
{
    public GetOrderByIdCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId cannot be empty");
    }
}

