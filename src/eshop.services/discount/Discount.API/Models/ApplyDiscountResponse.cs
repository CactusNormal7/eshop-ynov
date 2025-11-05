namespace Discount.API.Models;

/// <summary>
/// Réponse après application d'une réduction.
/// </summary>
public class ApplyDiscountResponse
{
    /// <summary>
    /// Montant total du panier avant réduction
    /// </summary>
    public decimal OriginalTotal { get; set; }
    
    /// <summary>
    /// Montant de la réduction appliquée
    /// </summary>
    public decimal DiscountAmount { get; set; }
    
    /// <summary>
    /// Montant total après réduction
    /// </summary>
    public decimal FinalTotal { get; set; }
    
    /// <summary>
    /// Code appliqué (si applicable)
    /// </summary>
    public string? AppliedCode { get; set; }
    
    /// <summary>
    /// Détails des réductions appliquées
    /// </summary>
    public List<DiscountDetail> AppliedDiscounts { get; set; } = [];
}

/// <summary>
/// Détail d'une réduction appliquée.
/// </summary>
public class DiscountDetail
{
    public string Type { get; set; } = string.Empty; // "Coupon", "Code", "Automatic"
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double Percentage { get; set; }
}

