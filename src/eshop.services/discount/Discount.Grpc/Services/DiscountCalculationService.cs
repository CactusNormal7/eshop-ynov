using Discount.Grpc.Models;

namespace Discount.Grpc.Services;

/// <summary>
/// Service utilitaire pour calculer les réductions selon les règles métier
/// </summary>
public class DiscountCalculationService
{
    /// <summary>
    /// Calcule le montant de réduction pour un coupon donné sur un prix
    /// </summary>
    public static double CalculateDiscountAmount(Coupon coupon, double originalPrice)
    {
        return coupon.DiscountType switch
        {
            Models.DiscountType.FixedAmount => coupon.Amount,
            Models.DiscountType.Percentage => originalPrice * (coupon.Percentage ?? 0) / 100.0,
            Models.DiscountType.FixedAmountWithCode => coupon.Amount + (originalPrice * (coupon.Percentage ?? 0) / 100.0),
            Models.DiscountType.Tiered => CalculateTieredDiscount(coupon, originalPrice),
            _ => 0
        };
    }
    
    /// <summary>
    /// Calcule le prix après réduction
    /// </summary>
    public static double CalculateDiscountedPrice(Coupon coupon, double originalPrice)
    {
        var discountAmount = CalculateDiscountAmount(coupon, originalPrice);
        var discountedPrice = originalPrice - discountAmount;
        return Math.Max(0, discountedPrice); // Ne pas aller en dessous de 0
    }
    
    /// <summary>
    /// Valide si un coupon peut être appliqué au panier
    /// </summary>
    public static (bool IsValid, string Message) ValidateCoupon(Coupon coupon, double cartTotal, bool isCodeRequired = false)
    {
        if (coupon.Status != Models.CouponStatus.Active)
        {
            return (false, $"Le coupon n'est pas actif. Statut: {coupon.Status}");
        }
        
        var now = DateTime.UtcNow;
        
        if (coupon.StartDate.HasValue && now < coupon.StartDate.Value)
        {
            return (false, "Le coupon n'est pas encore valide");
        }
        
        if (coupon.EndDate.HasValue && now > coupon.EndDate.Value)
        {
            return (false, "Le coupon a expiré");
        }
        
        if (coupon.MinimumAmount.HasValue && cartTotal < coupon.MinimumAmount.Value)
        {
            return (false, $"Le montant minimum du panier est {coupon.MinimumAmount.Value}");
        }
        
        if (coupon.RemainingUses >= 0 && coupon.RemainingUses == 0)
        {
            return (false, "Le coupon a atteint sa limite d'utilisations");
        }
        
        if (isCodeRequired && string.IsNullOrWhiteSpace(coupon.Code) && !coupon.IsAutomatic)
        {
            return (false, "Un code promo est requis pour ce coupon");
        }
        
        return (true, "Coupon valide");
    }
    
    /// <summary>
    /// Calcule la réduction totale cumulée avec validation du maximum autorisé
    /// </summary>
    public static (double TotalDiscount, double FinalPrice, List<Coupon> AppliedCoupons) CalculateCumulativeDiscount(
        List<Coupon> coupons, 
        double originalPrice, 
        double cartTotal,
        double? maxStackablePercentage = null)
    {
        var appliedCoupons = new List<Coupon>();
        var totalDiscount = 0.0;
        var totalPercentage = 0.0;
        
        // Trier les coupons : d'abord les montants fixes, puis les pourcentages
        var sortedCoupons = coupons
            .Where(c => ValidateCoupon(c, cartTotal).IsValid)
            .OrderBy(c => c.DiscountType == Models.DiscountType.FixedAmount ? 0 : 1)
            .ToList();
        
        foreach (var coupon in sortedCoupons)
        {
            if (!coupon.IsStackable && appliedCoupons.Any())
            {
                continue; // Skip si non cumulable et qu'un coupon est déjà appliqué
            }
            
            var couponDiscount = CalculateDiscountAmount(coupon, originalPrice);
            var couponPercentage = coupon.DiscountType == Models.DiscountType.Percentage 
                ? (coupon.Percentage ?? 0) 
                : (couponDiscount / originalPrice * 100);
            
            // Vérifier le maximum cumulable
            var newTotalPercentage = totalPercentage + couponPercentage;
            var maxAllowed = maxStackablePercentage ?? coupon.MaxStackablePercentage ?? 100;
            
            if (newTotalPercentage > maxAllowed)
            {
                // Ajuster le pourcentage pour respecter le maximum
                couponPercentage = Math.Max(0, maxAllowed - totalPercentage);
                couponDiscount = originalPrice * couponPercentage / 100.0;
            }
            
            totalDiscount += couponDiscount;
            totalPercentage += couponPercentage;
            appliedCoupons.Add(coupon);
            
            if (totalPercentage >= maxAllowed)
            {
                break; // Maximum atteint
            }
        }
        
        var finalPrice = Math.Max(0, originalPrice - totalDiscount);
        
        return (totalDiscount, finalPrice, appliedCoupons);
    }
    
    /// <summary>
    /// Calcule la réduction par paliers (Tiered)
    /// </summary>
    private static double CalculateTieredDiscount(Coupon coupon, double originalPrice)
    {
        // Pour l'instant, retourne le montant fixe
        // À étendre avec la logique de paliers si nécessaire
        return coupon.Amount;
    }
    
    /// <summary>
    /// Met à jour le statut d'un coupon en fonction des dates
    /// </summary>
    public static Models.CouponStatus UpdateCouponStatus(Coupon coupon)
    {
        var now = DateTime.UtcNow;
        
        if (coupon.Status == Models.CouponStatus.Disabled)
        {
            return Models.CouponStatus.Disabled;
        }
        
        if (coupon.StartDate.HasValue && now < coupon.StartDate.Value)
        {
            return Models.CouponStatus.Upcoming;
        }
        
        if (coupon.EndDate.HasValue && now > coupon.EndDate.Value)
        {
            return Models.CouponStatus.Expired;
        }
        
        return Models.CouponStatus.Active;
    }
}

