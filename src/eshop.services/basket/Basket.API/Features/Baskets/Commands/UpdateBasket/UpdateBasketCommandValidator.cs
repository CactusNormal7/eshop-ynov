using FluentValidation;

namespace Basket.API.Features.Baskets.Commands.UpdateBasket;

public class UpdateBasketCommandValidator : AbstractValidator<UpdateBasketCommand>
{
    public UpdateBasketCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("Username cannot be empty");
        RuleFor(x => x.Items).NotEmpty().WithMessage("Items cannot be empty");
        
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty().WithMessage("ProductId cannot be empty");
            item.RuleFor(i => i.ProductName).NotEmpty().WithMessage("ProductName cannot be empty");
            item.RuleFor(i => i.Price).GreaterThan(0).WithMessage("Price must be greater than 0");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0");
        });
        
        RuleFor(x => x.Items)
            .Must(items => items.GroupBy(i => i.ProductId).All(g => g.Count() == 1))
            .WithMessage("Duplicate ProductId found in items");
            
        RuleFor(x => x.Items)
            .Must(items => items.GroupBy(i => i.ProductName).All(g => g.Count() == 1))
            .WithMessage("Duplicate ProductName found in items");
    }
}
