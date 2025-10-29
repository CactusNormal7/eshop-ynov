namespace Discount.Grpc.Models;

/// <summary>
/// Représente un coupon de réduction avec support pour différents types de réductions,
/// codes promo, dates de validité et règles métier avancées.
/// </summary>
public class Coupon
{
    public int Id { get; set; }
    
    /// <summary>
    /// Nom du produit associé (optionnel, peut être null pour des réductions globales)
    /// </summary>
    public string? ProductName { get; set; }
    
    /// <summary>
    /// Catégorie de produit pour les réductions par catégorie (optionnel)
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// Description du coupon
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Code promo (optionnel pour les réductions automatiques)
    /// </summary>
    public string? Code { get; set; }
    
    /// <summary>
    /// Type de réduction: FixedAmount (montant fixe), Percentage (pourcentage), FixedAmountWithCode (montant fixe + code)
    /// </summary>
    public DiscountType DiscountType { get; set; } = DiscountType.FixedAmount;
    
    /// <summary>
    /// Montant de réduction fixe (en centimes ou unités de base)
    /// </summary>
    public double Amount { get; set; }
    
    /// <summary>
    /// Pourcentage de réduction (0-100)
    /// </summary>
    public double? Percentage { get; set; }
    
    /// <summary>
    /// Montant minimum du panier requis pour appliquer la réduction
    /// </summary>
    public double? MinimumAmount { get; set; }
    
    /// <summary>
    /// Date de début de validité du coupon
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Date de fin de validité du coupon
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Statut du coupon: Active, Expired, Disabled, Upcoming
    /// </summary>
    public CouponStatus Status { get; set; } = CouponStatus.Active;
    
    /// <summary>
    /// Indique si le coupon peut être cumulé avec d'autres réductions
    /// </summary>
    public bool IsStackable { get; set; } = true;
    
    /// <summary>
    /// Pourcentage maximum de réduction cumulée autorisée (ex: 30%)
    /// </summary>
    public double? MaxStackablePercentage { get; set; }
    
    /// <summary>
    /// Nombre d'utilisations restantes (-1 pour illimité)
    /// </summary>
    public int RemainingUses { get; set; } = -1;
    
    /// <summary>
    /// Indique si la réduction est automatique (sans code à saisir)
    /// </summary>
    public bool IsAutomatic { get; set; } = false;
    
    /// <summary>
    /// Date de création du coupon
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date de dernière modification
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Types de réductions supportés
/// </summary>
public enum DiscountType
{
    /// <summary>Réduction en montant fixe</summary>
    FixedAmount = 0,
    /// <summary>Réduction en pourcentage</summary>
    Percentage = 1,
    /// <summary>Réduction combinée: pourcentage + montant fixe avec code</summary>
    FixedAmountWithCode = 2,
    /// <summary>Réduction par paliers (montant variable selon seuil)</summary>
    Tiered = 3
}

/// <summary>
/// Statuts possibles d'un coupon
/// </summary>
public enum CouponStatus
{
    /// <summary>Actif et utilisable</summary>
    Active = 0,
    /// <summary>Expiré (date de fin dépassée)</summary>
    Expired = 1,
    /// <summary>Désactivé manuellement</summary>
    Disabled = 2,
    /// <summary>Pas encore actif (date de début future)</summary>
    Upcoming = 3
}