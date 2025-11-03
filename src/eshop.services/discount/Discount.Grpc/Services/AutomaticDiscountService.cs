using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Services;

/// <summary>
/// Service pour gérer les réductions automatiques (Black Friday, soldes, etc.).
/// </summary>
public class AutomaticDiscountService
{
    private readonly DiscountContext _dbContext;
    private readonly ILogger<AutomaticDiscountService> _logger;
    
    public AutomaticDiscountService(DiscountContext dbContext, ILogger<AutomaticDiscountService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    /// <summary>
    /// Récupère toutes les réductions automatiques actives selon le type.
    /// </summary>
    public async Task<List<Code>> GetActiveAutomaticDiscountsAsync(string? automaticType = null)
    {
        var now = DateTime.UtcNow;
        
        var query = _dbContext.Codes
            .Where(c => c.IsAutomatic && 
                       c.Status == "Active" &&
                       (!c.StartDate.HasValue || c.StartDate <= now) &&
                       (!c.EndDate.HasValue || c.EndDate >= now));
        
        if (!string.IsNullOrWhiteSpace(automaticType))
        {
            query = query.Where(c => c.AutomaticType == automaticType);
        }
        
        return await query.ToListAsync();
    }
    
    /// <summary>
    /// Récupère les réductions automatiques applicables à une catégorie de produit.
    /// </summary>
    public async Task<List<Code>> GetAutomaticDiscountsForCategoryAsync(string category, string? automaticType = null)
    {
        var discounts = await GetActiveAutomaticDiscountsAsync(automaticType);
        
        return discounts
            .Where(d => d.ApplicableCategories == null || 
                       !d.ApplicableCategories.Any() || 
                       d.ApplicableCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
            .ToList();
    }
    
    /// <summary>
    /// Récupère les réductions automatiques applicables selon le montant du panier (paliers).
    /// </summary>
    public async Task<List<Code>> GetAutomaticDiscountsForCartTotalAsync(decimal cartTotal)
    {
        var discounts = await GetActiveAutomaticDiscountsAsync();
        
        return discounts
            .Where(d => d.MinimumPurchaseAmount == 0 || (decimal)d.MinimumPurchaseAmount <= cartTotal)
            .ToList();
    }
}

