namespace Discount.API.Models;

/// <summary>
/// Requête pour créer un nouveau coupon.
/// </summary>
public class CreateCouponRequest
{
    /// <summary>
    /// Nom du produit concerné par la réduction
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>
    /// ID du produit (pour compatibilité avec Catalog Service)
    /// </summary>
    public Guid? ProductId { get; set; }
    
    /// <summary>
    /// Description du coupon
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
    /// Catégories de produits concernées (null = toutes les catégories)
    /// </summary>
    public List<string>? ApplicableCategories { get; set; }
    
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
}

/// <summary>
/// Requête pour mettre à jour un coupon existant.
/// </summary>
public class UpdateCouponRequest : CreateCouponRequest
{
    /// <summary>
    /// ID du coupon à mettre à jour
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Statut : Active, Expired, Disabled, Upcoming
    /// </summary>
    public string? Status { get; set; }
}

/// <summary>
/// Réponse avec les informations d'un coupon.
/// </summary>
public class CouponResponse
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid? ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public double Amount { get; set; }
    public double Percentage { get; set; }
    public List<string>? ApplicableCategories { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public double MinimumPurchaseAmount { get; set; }
}

