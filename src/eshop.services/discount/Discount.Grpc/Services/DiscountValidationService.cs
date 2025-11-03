using Discount.Grpc.Models;

namespace Discount.Grpc.Services;

/// <summary>
/// Service pour valider les codes de réduction et coupons.
/// </summary>
public class DiscountValidationService
{
    /// <summary>
    /// Valide un code de réduction selon ses règles métier.
    /// </summary>
    public static ValidationResult ValidateCode(Code code, decimal cartTotal, List<string>? productCategories = null)
    {
        var result = new ValidationResult { IsValid = true };
        
        // Vérifier le statut
        var now = DateTime.UtcNow;
        if (code.Status == "Expired" || code.Status == "Disabled")
        {
            result.IsValid = false;
            result.ErrorMessage = $"Le code {code.CodeValue} n'est plus valide (statut: {code.Status})";
            return result;
        }
        
        // Vérifier les dates
        if (code.StartDate.HasValue && now < code.StartDate.Value)
        {
            result.IsValid = false;
            result.ErrorMessage = $"Le code {code.CodeValue} sera valide à partir du {code.StartDate.Value:dd/MM/yyyy}";
            return result;
        }
        
        if (code.EndDate.HasValue && now > code.EndDate.Value)
        {
            result.IsValid = false;
            result.ErrorMessage = $"Le code {code.CodeValue} a expiré le {code.EndDate.Value:dd/MM/yyyy}";
            return result;
        }
        
        // Vérifier le montant minimum
        if (code.MinimumPurchaseAmount > 0 && (decimal)code.MinimumPurchaseAmount > cartTotal)
        {
            result.IsValid = false;
            result.ErrorMessage = $"Montant minimum requis: {code.MinimumPurchaseAmount:F2}€";
            return result;
        }
        
        // Vérifier les catégories
        if (code.ApplicableCategories != null && 
            code.ApplicableCategories.Any() && 
            productCategories != null &&
            productCategories.Any())
        {
            var hasApplicableCategory = code.ApplicableCategories
                .Any(cat => productCategories.Contains(cat, StringComparer.OrdinalIgnoreCase));
            
            if (!hasApplicableCategory)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Ce code n'est pas applicable aux catégories de produits de votre panier";
                return result;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Valide un coupon selon ses règles métier.
    /// </summary>
    public static ValidationResult ValidateCoupon(Coupon coupon, decimal cartTotal, List<string>? productCategories = null)
    {
        var result = new ValidationResult { IsValid = true };
        
        // Vérifier le statut
        var now = DateTime.UtcNow;
        if (coupon.Status == "Expired" || coupon.Status == "Disabled")
        {
            result.IsValid = false;
            result.ErrorMessage = "Ce coupon n'est plus valide";
            return result;
        }
        
        // Vérifier les dates
        if (coupon.StartDate.HasValue && now < coupon.StartDate.Value)
        {
            result.IsValid = false;
            result.ErrorMessage = $"Ce coupon sera valide à partir du {coupon.StartDate.Value:dd/MM/yyyy}";
            return result;
        }
        
        if (coupon.EndDate.HasValue && now > coupon.EndDate.Value)
        {
            result.IsValid = false;
            result.ErrorMessage = $"Ce coupon a expiré le {coupon.EndDate.Value:dd/MM/yyyy}";
            return result;
        }
        
        // Vérifier le montant minimum
        if (coupon.MinimumPurchaseAmount > 0 && (decimal)coupon.MinimumPurchaseAmount > cartTotal)
        {
            result.IsValid = false;
            result.ErrorMessage = $"Montant minimum requis: {coupon.MinimumPurchaseAmount:F2}€";
            return result;
        }
        
        // Vérifier les catégories
        if (coupon.ApplicableCategories != null && 
            coupon.ApplicableCategories.Any() && 
            productCategories != null &&
            productCategories.Any())
        {
            var hasApplicableCategory = coupon.ApplicableCategories
                .Any(cat => productCategories.Contains(cat, StringComparer.OrdinalIgnoreCase));
            
            if (!hasApplicableCategory)
            {
                result.IsValid = false;
                result.ErrorMessage = "Ce coupon n'est pas applicable aux catégories de produits de votre panier";
                return result;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Met à jour le statut d'un code selon ses dates.
    /// </summary>
    public static void UpdateCodeStatus(Code code)
    {
        var now = DateTime.UtcNow;
        
        if (code.Status == "Disabled")
            return; // Ne pas modifier un code désactivé manuellement
        
        if (code.EndDate.HasValue && now > code.EndDate.Value)
        {
            code.Status = "Expired";
        }
        else if (code.StartDate.HasValue && now < code.StartDate.Value)
        {
            code.Status = "Upcoming";
        }
        else
        {
            code.Status = "Active";
        }
    }
    
    /// <summary>
    /// Met à jour le statut d'un coupon selon ses dates.
    /// </summary>
    public static void UpdateCouponStatus(Coupon coupon)
    {
        var now = DateTime.UtcNow;
        
        if (coupon.Status == "Disabled")
            return; // Ne pas modifier un coupon désactivé manuellement
        
        if (coupon.EndDate.HasValue && now > coupon.EndDate.Value)
        {
            coupon.Status = "Expired";
        }
        else if (coupon.StartDate.HasValue && now < coupon.StartDate.Value)
        {
            coupon.Status = "Upcoming";
        }
        else
        {
            coupon.Status = "Active";
        }
    }
}

/// <summary>
/// Résultat de validation d'un code ou coupon.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

