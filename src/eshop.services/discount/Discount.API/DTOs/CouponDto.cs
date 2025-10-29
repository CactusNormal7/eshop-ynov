namespace Discount.API.DTOs;

/// <summary>
/// DTO pour représenter un coupon de réduction
/// </summary>
public class CouponDto
{
    public int Id { get; set; }
    public string? ProductName { get; set; }
    public string? Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string DiscountType { get; set; } = string.Empty; // "FixedAmount", "Percentage", etc.
    public double Amount { get; set; }
    public double? Percentage { get; set; }
    public double? MinimumAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Active"; // "Active", "Expired", "Disabled", "Upcoming"
    public bool IsStackable { get; set; } = true;
    public double? MaxStackablePercentage { get; set; }
    public int RemainingUses { get; set; } = -1;
    public bool IsAutomatic { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO pour créer un coupon
/// </summary>
public class CreateCouponDto
{
    public string? ProductName { get; set; }
    public string? Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string DiscountType { get; set; } = "FixedAmount";
    public double Amount { get; set; }
    public double? Percentage { get; set; }
    public double? MinimumAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsStackable { get; set; } = true;
    public double? MaxStackablePercentage { get; set; }
    public int RemainingUses { get; set; } = -1;
    public bool IsAutomatic { get; set; } = false;
}

/// <summary>
/// DTO pour mettre à jour un coupon
/// </summary>
public class UpdateCouponDto
{
    public int Id { get; set; }
    public string? ProductName { get; set; }
    public string? Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string DiscountType { get; set; } = "FixedAmount";
    public double Amount { get; set; }
    public double? Percentage { get; set; }
    public double? MinimumAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsStackable { get; set; } = true;
    public double? MaxStackablePercentage { get; set; }
    public int RemainingUses { get; set; } = -1;
    public bool IsAutomatic { get; set; } = false;
}

/// <summary>
/// DTO pour valider un code promo
/// </summary>
public class ValidateCouponCodeDto
{
    public string Code { get; set; } = string.Empty;
    public double CartTotal { get; set; }
}

/// <summary>
/// Réponse de validation d'un code promo
/// </summary>
public class ValidateCouponCodeResponseDto
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public CouponDto? Coupon { get; set; }
}

/// <summary>
/// DTO pour appliquer une réduction
/// </summary>
public class ApplyDiscountDto
{
    public string ProductName { get; set; } = string.Empty;
    public double OriginalPrice { get; set; }
    public string? DiscountCode { get; set; }
    public double CartTotal { get; set; }
}

/// <summary>
/// Réponse d'application d'une réduction
/// </summary>
public class ApplyDiscountResponseDto
{
    public double DiscountedPrice { get; set; }
    public double DiscountAmount { get; set; }
    public double DiscountPercentage { get; set; }
    public List<CouponDto> AppliedCoupons { get; set; } = new();
}

/// <summary>
/// DTO pour calculer la réduction totale du panier
/// </summary>
public class CalculateTotalDiscountDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public string? DiscountCode { get; set; }
    public double CartTotal { get; set; }
}

/// <summary>
/// DTO pour un item du panier
/// </summary>
public class CartItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public double Price { get; set; }
    public int Quantity { get; set; }
    public string? Category { get; set; }
}

/// <summary>
/// Réponse du calcul de réduction totale
/// </summary>
public class CalculateTotalDiscountResponseDto
{
    public double TotalDiscount { get; set; }
    public double FinalTotal { get; set; }
    public List<CouponDto> AppliedCoupons { get; set; } = new();
}

/// <summary>
/// DTO pour la pagination des coupons
/// </summary>
public class PaginatedCouponsDto
{
    public List<CouponDto> Coupons { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (PageSize * 1.0));
}
