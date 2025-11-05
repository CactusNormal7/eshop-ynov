namespace Discount.API.Models;

/// <summary>
/// Réponse contenant les réductions applicables à un produit.
/// </summary>
public class ProductDiscountResponse
{
    /// <summary>
    /// ID du produit
    /// </summary>
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Nom du produit
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>
    /// Coupon applicable au produit
    /// </summary>
    public CouponInfo? Coupon { get; set; }
    
    /// <summary>
    /// Réductions automatiques applicables
    /// </summary>
    public List<AutomaticDiscountInfo> AutomaticDiscounts { get; set; } = [];
}

/// <summary>
/// Information sur un coupon.
/// </summary>
public class CouponInfo
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public double Percentage { get; set; }
    public double Amount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Information sur une réduction automatique.
/// </summary>
public class AutomaticDiscountInfo
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Percentage { get; set; }
    public double Amount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

