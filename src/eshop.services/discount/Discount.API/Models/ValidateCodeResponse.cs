namespace Discount.API.Models;

/// <summary>
/// Réponse de validation d'un code promo.
/// </summary>
public class ValidateCodeResponse
{
    /// <summary>
    /// Indique si le code est valide
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Message d'erreur si le code n'est pas valide
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Informations sur le code si valide
    /// </summary>
    public CodeInfo? CodeInfo { get; set; }
}

/// <summary>
/// Informations sur un code promo.
/// </summary>
public class CodeInfo
{
    public string CodeValue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Percentage { get; set; }
    public double Amount { get; set; }
    public double MinimumPurchaseAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsStackable { get; set; }
    public double? MaxCumulativeDiscountPercentage { get; set; }
}

