using FluentValidation;

namespace Ordering.Application.Features.Orders.Commands.GetOrdersByCustomerId;

public class GetOrdersByCustomerIdCommandValidator : AbstractValidator<GetOrdersByCustomerIdCommand>
{
    public GetOrdersByCustomerIdCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("CustomerId cannot be empty");
    }
}

