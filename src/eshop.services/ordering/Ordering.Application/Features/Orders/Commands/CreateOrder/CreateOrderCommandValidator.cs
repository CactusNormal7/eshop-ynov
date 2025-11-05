using FluentValidation;

namespace Ordering.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Validator for the CreateOrderCommand.
/// Validates the required fields and their constraints within the CreateOrderCommand.
/// </summary>
/// <remarks>
/// This class inherits from the FluentValidation AbstractValidator, enabling detailed validation rules
/// for the CreateOrderCommand. Specifically, it enforces the following:
/// - Ensures that the OrderName in the associated Order is not empty.
/// - Ensures that the CustomerId in the associated Order is not empty.
/// - Ensures that the OrderItems list in the associated Order is not empty.
/// - Validates the shipping and billing addresses.
/// - Validates the payment information.
/// </remarks>
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Order.OrderName).NotEmpty().WithMessage("OrderName is required");
        RuleFor(x => x.Order.CustomerId).NotEmpty().WithMessage("CustomerId is required");
        RuleFor(x => x.Order.OrderItems).NotEmpty().WithMessage("OrderItems should not be empty");
        
        RuleFor(x => x.Order.ShippingAddress).NotNull().WithMessage("ShippingAddress is required");
        RuleFor(x => x.Order.BillingAddress).NotNull().WithMessage("BillingAddress is required");
        RuleFor(x => x.Order.Payment).NotNull().WithMessage("Payment is required");
        
        When(x => x.Order.ShippingAddress is not null, () =>
        {
            RuleFor(x => x.Order.ShippingAddress!.EmailAddress).NotEmpty().WithMessage("ShippingAddress EmailAddress is required");
            RuleFor(x => x.Order.ShippingAddress!.AddressLine).NotEmpty().WithMessage("ShippingAddress AddressLine is required");
        });
        
        When(x => x.Order.BillingAddress is not null, () =>
        {
            RuleFor(x => x.Order.BillingAddress!.EmailAddress).NotEmpty().WithMessage("BillingAddress EmailAddress is required");
            RuleFor(x => x.Order.BillingAddress!.AddressLine).NotEmpty().WithMessage("BillingAddress AddressLine is required");
        });
        
        When(x => x.Order.Payment is not null, () =>
        {
            RuleFor(x => x.Order.Payment!.CardName).NotEmpty().WithMessage("Payment CardName is required");
            RuleFor(x => x.Order.Payment!.CardNumber).NotEmpty().WithMessage("Payment CardNumber is required");
            RuleFor(x => x.Order.Payment!.Expiration).NotEmpty().WithMessage("Payment Expiration is required");
            RuleFor(x => x.Order.Payment!.Cvv).NotEmpty().Length(3).WithMessage("Payment Cvv must be 3 digits");
        });
        
        RuleForEach(x => x.Order.OrderItems).ChildRules(items =>
        {
            items.RuleFor(x => x.ProductId).NotEmpty().WithMessage("OrderItem ProductId is required");
            items.RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("OrderItem Quantity must be greater than 0");
            items.RuleFor(x => x.Price).GreaterThan(0).WithMessage("OrderItem Price must be greater than 0");
        });
    }
}