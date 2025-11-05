using Discount.API.Models;
using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Discount.Grpc.Services;
using Microsoft.EntityFrameworkCore;

namespace Discount.API.Services;

/// <summary>
/// Service pour appliquer les réductions aux paniers.
/// </summary>
public class DiscountApplicationService
{
    private readonly DiscountContext _dbContext;
    private readonly AutomaticDiscountService _automaticDiscountService;
    private readonly ILogger<DiscountApplicationService> _logger;
    
    public DiscountApplicationService(
        DiscountContext dbContext,
        AutomaticDiscountService automaticDiscountService,
        ILogger<DiscountApplicationService> logger)
    {
        _dbContext = dbContext;
        _automaticDiscountService = automaticDiscountService;
        _logger = logger;
    }
    
    /// <summary>
    /// Applique les réductions disponibles à un panier.
    /// </summary>
    public async Task<ApplyDiscountResponse> ApplyDiscountsAsync(ApplyDiscountRequest request)
    {
        var response = new ApplyDiscountResponse
        {
            OriginalTotal = request.CartTotal,
            FinalTotal = request.CartTotal
        };
        
        var allCategories = request.Items
            .SelectMany(i => i.Categories)
            .Distinct()
            .ToList();
        
        // Récupérer les réductions automatiques applicables
        var automaticDiscounts = await _automaticDiscountService.GetAutomaticDiscountsForCartTotalAsync(request.CartTotal);
        automaticDiscounts = automaticDiscounts
            .Where(d => d.ApplicableCategories == null || 
                       !d.ApplicableCategories.Any() ||
                       d.ApplicableCategories.Any(c => allCategories.Contains(c, StringComparer.OrdinalIgnoreCase)))
            .ToList();
        
        // Calculer la réduction automatique
        decimal automaticDiscountAmount = 0;
        foreach (var autoDiscount in automaticDiscounts)
        {
            var discountValue = DiscountCalculationService.ApplyDiscount(
                request.CartTotal,
                autoDiscount.Percentage,
                autoDiscount.Amount);
            
            // Appliquer les paliers si présents
            if (!string.IsNullOrWhiteSpace(autoDiscount.TierRules))
            {
                var tierPercentage = DiscountCalculationService.CalculateTierDiscountPercentage(
                    request.CartTotal,
                    autoDiscount.TierRules);
                discountValue = request.CartTotal * (decimal)(tierPercentage / 100.0);
            }
            
            automaticDiscountAmount += discountValue;
            
            response.AppliedDiscounts.Add(new DiscountDetail
            {
                Type = "Automatic",
                Description = autoDiscount.Description,
                Amount = discountValue,
                Percentage = autoDiscount.Percentage
            });
        }
        
        // Appliquer le code promo si fourni
        decimal codeDiscountAmount = 0;
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = await _dbContext.Codes
                .FirstOrDefaultAsync(c => c.CodeValue == request.Code);
            
            if (code != null)
            {
                var validation = DiscountValidationService.ValidateCode(
                    code,
                    request.CartTotal,
                    allCategories);
                
                if (validation.IsValid)
                {
                    codeDiscountAmount = DiscountCalculationService.ApplyDiscount(
                        request.CartTotal - automaticDiscountAmount,
                        code.Percentage,
                        code.Amount);
                    
                    response.AppliedCode = code.CodeValue;
                    response.AppliedDiscounts.Add(new DiscountDetail
                    {
                        Type = "Code",
                        Description = code.Description,
                        Amount = codeDiscountAmount,
                        Percentage = code.Percentage
                    });
                }
            }
        }
        
        // Appliquer les coupons produits
        decimal couponDiscountAmount = 0;
        foreach (var item in request.Items)
        {
            Coupon? coupon = null;
            
            if (item.ProductId.HasValue)
            {
                coupon = await _dbContext.Coupons
                    .FirstOrDefaultAsync(c => c.ProductId == item.ProductId.Value);
            }
            
            if (coupon == null && !string.IsNullOrWhiteSpace(item.ProductName))
            {
                coupon = await _dbContext.Coupons
                    .FirstOrDefaultAsync(c => c.ProductName == item.ProductName);
            }
            
            if (coupon != null)
            {
                var validation = DiscountValidationService.ValidateCoupon(
                    coupon,
                    request.CartTotal,
                    item.Categories);
                
                if (validation.IsValid)
                {
                    var itemTotal = item.Price * item.Quantity;
                    var itemDiscount = DiscountCalculationService.ApplyDiscount(
                        itemTotal,
                        coupon.Percentage,
                        coupon.Amount);
                    
                    couponDiscountAmount += itemDiscount;
                    
                    response.AppliedDiscounts.Add(new DiscountDetail
                    {
                        Type = "Coupon",
                        Description = $"{coupon.Description} - {item.ProductName}",
                        Amount = itemDiscount,
                        Percentage = coupon.Percentage
                    });
                }
            }
        }
        
        // Calculer le total final avec règles de cumulabilité
        var totalDiscountBeforeCumulative = automaticDiscountAmount + codeDiscountAmount + couponDiscountAmount;
        
        // Vérifier les limites de cumulabilité
        var maxCumulativeDiscount = request.CartTotal * 0.30m; // 30% par défaut
        var codesWithMaxCumulative = await _dbContext.Codes
            .Where(c => c.MaxCumulativeDiscountPercentage.HasValue)
            .ToListAsync();
        
        if (codesWithMaxCumulative.Any())
        {
            var minMaxPercentage = codesWithMaxCumulative
                .Min(c => c.MaxCumulativeDiscountPercentage ?? 100);
            maxCumulativeDiscount = request.CartTotal * (decimal)(minMaxPercentage / 100.0);
        }
        
        var finalDiscountAmount = Math.Min(totalDiscountBeforeCumulative, maxCumulativeDiscount);
        finalDiscountAmount = Math.Min(finalDiscountAmount, request.CartTotal);
        
        response.DiscountAmount = finalDiscountAmount;
        response.FinalTotal = request.CartTotal - finalDiscountAmount;
        
        _logger.LogInformation(
            "Applied discounts: Original={OriginalTotal}, Discount={DiscountAmount}, Final={FinalTotal}",
            response.OriginalTotal,
            response.DiscountAmount,
            response.FinalTotal);
        
        return response;
    }
}

