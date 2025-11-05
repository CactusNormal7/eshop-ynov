namespace Discount.Grpc.Models;

/// <summary>
/// Représente un code de réduction avec toutes ses propriétés métier.
/// </summary>
public class Code
{
    public int Id { get; set; }
    
    /// <summary>
    /// Code unique du coupon (ex: "BLACKFRIDAY2024")
    /// </summary>
    public string CodeValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Description du code promo
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Règles de cumulabilité (JSON ou texte)
    /// </summary>
    public string Rules { get; set; } = string.Empty;
    
    /// <summary>
    /// Montant fixe de réduction
    /// </summary>
    public double Amount { get; set; }
    
    /// <summary>
    /// Pourcentage de réduction
    /// </summary>
    public double Percentage { get; set; }
    
    /// <summary>
    /// Statut du code : Active, Expired, Disabled, Upcoming
    /// </summary>
    public string Status { get; set; } = "Active";
    
    /// <summary>
    /// Date de début de validité
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Date de fin de validité
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Montant minimum d'achat requis pour utiliser ce code
    /// </summary>
    public double MinimumPurchaseAmount { get; set; }
    
    /// <summary>
    /// Catégories de produits concernées (null = toutes les catégories)
    /// </summary>
    public List<string>? ApplicableCategories { get; set; }
    
    /// <summary>
    /// Indique si le code peut être cumulé avec d'autres réductions
    /// </summary>
    public bool IsStackable { get; set; }
    
    /// <summary>
    /// Pourcentage maximum de réduction cumulée autorisée (ex: 30%)
    /// </summary>
    public double? MaxCumulativeDiscountPercentage { get; set; }
    
    /// <summary>
    /// Indique si c'est une réduction automatique (pas besoin de saisir le code)
    /// </summary>
    public bool IsAutomatic { get; set; }
    
    /// <summary>
    /// Type de réduction automatique (ex: "BlackFriday", "SummerSale")
    /// </summary>
    public string? AutomaticType { get; set; }
    
    /// <summary>
    /// Paliers de réduction (JSON serialisé, ex: [{"threshold": 100, "percentage": 5}, {"threshold": 200, "percentage": 10}])
    /// </summary>
    public string? TierRules { get; set; }
}
