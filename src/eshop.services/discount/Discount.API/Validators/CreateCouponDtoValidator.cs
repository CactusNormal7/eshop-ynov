using Discount.API.DTOs;
using FluentValidation;

namespace Discount.API.Validators;

/// <summary>
/// Validateur pour CreateCouponDto
/// </summary>
public class CreateCouponDtoValidator : AbstractValidator<CreateCouponDto>
{
    public CreateCouponDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La description est requise")
            .MaximumLength(500).WithMessage("La description ne peut pas dépasser 500 caractères");

        RuleFor(x => x.DiscountType)
            .NotEmpty().WithMessage("Le type de réduction est requis")
            .Must(BeValidDiscountType).WithMessage("Le type de réduction doit être: FixedAmount, Percentage, FixedAmountWithCode, ou Tiered");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Le montant doit être positif");

        RuleFor(x => x.Percentage)
            .Must(percentage => !percentage.HasValue || (percentage.Value >= 0 && percentage.Value <= 100))
            .WithMessage("Le pourcentage doit être entre 0 et 100");

        RuleFor(x => x.MinimumAmount)
            .GreaterThanOrEqualTo(0).When(x => x.MinimumAmount.HasValue)
            .WithMessage("Le montant minimum doit être positif");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Le code ne peut pas dépasser 50 caractères")
            .Must(BeValidCode).When(x => !string.IsNullOrWhiteSpace(x.Code))
            .WithMessage("Le code ne peut contenir que des lettres, chiffres et tirets");

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("La date de début doit être avant la date de fin");

        RuleFor(x => x.MaxStackablePercentage)
            .Must(percentage => !percentage.HasValue || (percentage.Value >= 0 && percentage.Value <= 100))
            .WithMessage("Le pourcentage maximum cumulable doit être entre 0 et 100");

        RuleFor(x => x.RemainingUses)
            .GreaterThanOrEqualTo(-1).WithMessage("Le nombre d'utilisations restantes doit être -1 (illimité) ou positif");
    }

    private bool BeValidDiscountType(string discountType)
    {
        var validTypes = new[] { "FixedAmount", "Percentage", "FixedAmountWithCode", "Tiered" };
        return validTypes.Contains(discountType, StringComparer.OrdinalIgnoreCase);
    }

    private bool BeValidCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return true;
            
        return System.Text.RegularExpressions.Regex.IsMatch(code, @"^[A-Za-z0-9-]+$");
    }
}

/// <summary>
/// Validateur pour UpdateCouponDto
/// </summary>
public class UpdateCouponDtoValidator : AbstractValidator<UpdateCouponDto>
{
    public UpdateCouponDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("L'ID doit être positif");

        Include(new CreateCouponDtoValidator());
    }
}

/// <summary>
/// Validateur pour ValidateCouponCodeDto
/// </summary>
public class ValidateCouponCodeDtoValidator : AbstractValidator<ValidateCouponCodeDto>
{
    public ValidateCouponCodeDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Le code promo est requis")
            .MaximumLength(50).WithMessage("Le code ne peut pas dépasser 50 caractères");

        RuleFor(x => x.CartTotal)
            .GreaterThanOrEqualTo(0).WithMessage("Le montant du panier doit être positif");
    }
}

/// <summary>
/// Validateur pour ApplyDiscountDto
/// </summary>
public class ApplyDiscountDtoValidator : AbstractValidator<ApplyDiscountDto>
{
    public ApplyDiscountDtoValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Le nom du produit est requis");

        RuleFor(x => x.OriginalPrice)
            .GreaterThan(0).WithMessage("Le prix original doit être supérieur à 0");

        RuleFor(x => x.CartTotal)
            .GreaterThanOrEqualTo(0).WithMessage("Le montant du panier doit être positif");
    }
}

/// <summary>
/// Validateur pour CalculateTotalDiscountDto
/// </summary>
public class CalculateTotalDiscountDtoValidator : AbstractValidator<CalculateTotalDiscountDto>
{
    public CalculateTotalDiscountDtoValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La liste des articles ne peut pas être vide");

        RuleForEach(x => x.Items).SetValidator(new CartItemDtoValidator());

        RuleFor(x => x.CartTotal)
            .GreaterThanOrEqualTo(0).WithMessage("Le montant du panier doit être positif");
    }
}

/// <summary>
/// Validateur pour CartItemDto
/// </summary>
public class CartItemDtoValidator : AbstractValidator<CartItemDto>
{
    public CartItemDtoValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Le nom du produit est requis");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Le prix doit être supérieur à 0");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La quantité doit être supérieure à 0");
    }
}

