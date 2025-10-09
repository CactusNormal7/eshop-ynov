using FluentValidation;

namespace Catalog.API.Features.Products.Commands.CreateProduct;

/// <summary>
/// Validates the CreateProductCommand to ensure that all required properties meet the defined rules and constraints.
/// </summary>
/// <remarks>
/// Utilizes FluentValidation to define validation rules for properties of the CreateProductCommand.
/// This validator ensures that the data provided for creating a product is correct and adheres to business logic constraints.
/// </remarks>
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    /// <summary>
    /// Provides validation rules for the CreateProductCommand.
    /// </summary>
    /// <remarks>
    /// Ensures that the command meets necessary requirements such as non-empty properties
    /// and valid data constraints for creating a product.
    /// </remarks>
    public CreateProductCommandValidator()
    {
        RuleFor(product => product.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(product => product.Categories).NotEmpty().WithMessage("Categories are required");
        RuleFor(product => product.ImageFile).NotEmpty().WithMessage("ImageFile is required");
        RuleFor(product => product.Description).NotEmpty().WithMessage("Description is required");
        RuleFor(product => product.Price).GreaterThanOrEqualTo(1).WithMessage("Price must be greater than or equal to 1");
        RuleFor(product => product.Stock).GreaterThanOrEqualTo(0).WithMessage("Stock must be greater than or equal to 0");
    }
}