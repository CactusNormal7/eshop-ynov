using FluentValidation;

namespace Basket.API.Features.Baskets.Commands.AddProductToBasket;

/// <summary>
/// Validates the AddProductToBasketCommand to ensure that all required properties meet the defined rules and constraints.
/// </summary>
/// <remarks>
/// Utilizes FluentValidation to define validation rules for properties of the AddProductToBasketCommand.
/// This validator ensures that the data provided for adding a product to basket is correct and adheres to business logic constraints.
/// </remarks>
public class AddProductToBasketCommandValidator : AbstractValidator<AddProductToBasketCommand>
{
    /// <summary>
    /// Provides validation rules for the AddProductToBasketCommand.
    /// </summary>
    /// <remarks>
    /// Ensures that the command meets necessary requirements such as non-empty properties
    /// and valid data constraints for adding a product to the basket.
    /// </remarks>
    public AddProductToBasketCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(command => command.Categories).NotEmpty().WithMessage("Categories are required");
        RuleFor(command => command.Description).NotEmpty().WithMessage("Description is required");
        RuleFor(command => command.Color).NotEmpty().WithMessage("Color is required");
        RuleFor(command => command.Price).GreaterThanOrEqualTo(1).WithMessage("Price must be greater than or equal to 1");
        RuleFor(command => command.Quantity).GreaterThanOrEqualTo(1).WithMessage("Quantity must be greater than or equal to 1");
    }
}