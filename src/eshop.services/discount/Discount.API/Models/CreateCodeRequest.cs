namespace Discount.API.Models;

/// <summary>
/// Requête pour créer un nouveau code promo.
/// </summary>
public class CreateCodeRequest
{
    /// <summary>
    /// Code unique du coupon (ex: "BLACKFRIDAY2024")
    /// </summary>
    public string CodeValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Description du code promo
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Montant fixe de réduction
    /// </summary>
    public double Amount { get; set; }
    
    /// <summary>
    /// Pourcentage de réduction
    /// </summary>
    public double Percentage { get; set; }
    
    /// <summary>
    /// Date de début de validité
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Date de fin de validité
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Montant minimum d'achat requis
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

/// <summary>
/// Requête pour mettre à jour un code promo existant.
/// </summary>
public class UpdateCodeRequest : CreateCodeRequest
{
    /// <summary>
    /// ID du code à mettre à jour
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Statut du code : Active, Expired, Disabled, Upcoming
    /// </summary>
    public string? Status { get; set; }
}

/// <summary>
/// Réponse avec les informations d'un code promo.
/// </summary>
public class CodeResponse
{
    public int Id { get; set; }
    public string CodeValue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Amount { get; set; }
    public double Percentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public double MinimumPurchaseAmount { get; set; }
    public List<string>? ApplicableCategories { get; set; }
    public bool IsStackable { get; set; }
    public double? MaxCumulativeDiscountPercentage { get; set; }
    public bool IsAutomatic { get; set; }
    public string? AutomaticType { get; set; }
    public string? TierRules { get; set; }
}

