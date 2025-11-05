namespace Discount.Grpc.Models;

/// <summary>
/// Représente un coupon de réduction lié à un produit spécifique.
/// </summary>
public class Coupon
{
    public int Id { get; set; }
    
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
    /// Statut : Active, Expired, Disabled, Upcoming
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
    /// Montant minimum d'achat requis pour utiliser ce coupon
    /// </summary>
    public double MinimumPurchaseAmount { get; set; }
}