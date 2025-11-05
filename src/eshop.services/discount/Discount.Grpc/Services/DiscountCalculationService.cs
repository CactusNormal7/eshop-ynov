using Discount.Grpc.Models;

namespace Discount.Grpc.Services;

/// <summary>
/// Service pour calculer les réductions selon les règles métier.
/// </summary>
public class DiscountCalculationService
{
    /// <summary>
    /// Calcule la réduction totale en appliquant les règles de cumulabilité.
    /// </summary>
    /// <param name="baseAmount">Montant de base avant réduction</param>
    /// <param name="couponDiscount">Réduction du coupon produit</param>
    /// <param name="codeDiscount">Réduction du code promo</param>
    /// <param name="maxCumulativePercentage">Pourcentage maximum cumulé autorisé</param>
    /// <returns>Montant de la réduction totale</returns>
    public static decimal CalculateTotalDiscount(
        decimal baseAmount,
        decimal couponDiscount,
        decimal codeDiscount,
        double? maxCumulativePercentage = null)
    {
        var totalDiscount = couponDiscount + codeDiscount;
        
        // Vérifier le plafond de réduction cumulée
        if (maxCumulativePercentage.HasValue)
        {
            var maxDiscountAmount = baseAmount * (decimal)(maxCumulativePercentage.Value / 100.0);
            totalDiscount = Math.Min(totalDiscount, maxDiscountAmount);
        }
        
        // S'assurer que la réduction totale ne dépasse pas le montant de base
        return Math.Min(totalDiscount, baseAmount);
    }
    
    /// <summary>
    /// Calcule la réduction par paliers selon le montant du panier.
    /// </summary>
    /// <param name="cartTotal">Total du panier</param>
    /// <param name="tierRulesJson">JSON des règles de paliers</param>
    /// <returns>Pourcentage de réduction applicable</returns>
    public static double CalculateTierDiscountPercentage(decimal cartTotal, string? tierRulesJson)
    {
        if (string.IsNullOrWhiteSpace(tierRulesJson))
            return 0;
        
        try
        {
            var tiers = System.Text.Json.JsonSerializer.Deserialize<List<TierRule>>(tierRulesJson);
            if (tiers == null || !tiers.Any())
                return 0;
            
            // Trouver le palier le plus élevé applicable
            var applicableTier = tiers
                .Where(t => (decimal)t.Threshold <= cartTotal)
                .OrderByDescending(t => t.Threshold)
                .FirstOrDefault();
            
            return applicableTier?.Percentage ?? 0;
        }
        catch
        {
            return 0;
        }
    }
    
    /// <summary>
    /// Applique une réduction en pourcentage ou montant fixe.
    /// </summary>
    public static decimal ApplyDiscount(decimal amount, double percentage, double fixedAmount = 0)
    {
        var percentageDiscount = amount * (decimal)(percentage / 100.0);
        var totalDiscount = percentageDiscount + (decimal)fixedAmount;
        return Math.Min(totalDiscount, amount);
    }
}

/// <summary>
/// Représente une règle de palier pour les réductions.
/// </summary>
public class TierRule
{
    public double Threshold { get; set; }
    public double Percentage { get; set; }
}

