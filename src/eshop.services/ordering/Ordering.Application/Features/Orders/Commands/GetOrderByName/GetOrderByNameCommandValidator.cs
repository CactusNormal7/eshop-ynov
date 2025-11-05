using FluentValidation;

namespace Ordering.Application.Features.Orders.Commands.GetOrderByName;

public class GetOrderByNameCommandValidator : AbstractValidator<GetOrderByNameCommand>
{
    public GetOrderByNameCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name cannot be empty")
            .MinimumLength(5)
            .WithMessage("Name must be at least 5 characters");
    }
}

