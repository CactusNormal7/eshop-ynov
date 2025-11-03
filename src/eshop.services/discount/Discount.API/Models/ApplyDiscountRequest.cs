namespace Discount.API.Models;

/// <summary>
/// Requête pour appliquer une réduction à un panier.
/// </summary>
public class ApplyDiscountRequest
{
    /// <summary>
    /// Code promo à appliquer (optionnel si réduction automatique)
    /// </summary>
    public string? Code { get; set; }
    
    /// <summary>
    /// Total du panier avant réduction
    /// </summary>
    public decimal CartTotal { get; set; }
    
    /// <summary>
    /// Liste des produits dans le panier avec leurs informations
    /// </summary>
    public List<CartItem> Items { get; set; } = [];
}

/// <summary>
/// Représente un article du panier pour le calcul des réductions.
/// </summary>
public class CartItem
{
    /// <summary>
    /// Nom du produit
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>
    /// ID du produit
    /// </summary>
    public Guid? ProductId { get; set; }
    
    /// <summary>
    /// Catégories du produit
    /// </summary>
    public List<string> Categories { get; set; } = [];
    
    /// <summary>
    /// Prix unitaire
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Quantité
    /// </summary>
    public int Quantity { get; set; }
}

