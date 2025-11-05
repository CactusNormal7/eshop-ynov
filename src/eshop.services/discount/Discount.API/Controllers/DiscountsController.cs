using Discount.API.Models;
using Discount.API.Services;
using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Discount.Grpc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discount.API.Controllers;

/// <summary>
/// Contrôleur pour gérer les réductions et codes promo.
/// </summary>
[ApiController]
[Route("discounts")]
[Produces("application/json")]
public class DiscountsController : ControllerBase
{
    private readonly DiscountApplicationService _discountApplicationService;
    private readonly DiscountContext _dbContext;
    private readonly AutomaticDiscountService _automaticDiscountService;
    private readonly ILogger<DiscountsController> _logger;
    
    public DiscountsController(
        DiscountApplicationService discountApplicationService,
        DiscountContext dbContext,
        AutomaticDiscountService automaticDiscountService,
        ILogger<DiscountsController> logger)
    {
        _discountApplicationService = discountApplicationService;
        _dbContext = dbContext;
        _automaticDiscountService = automaticDiscountService;
        _logger = logger;
    }
    
    /// <summary>
    /// Applique un code promo ou des réductions automatiques à un panier.
    /// </summary>
    /// <param name="request">Les détails du panier et le code promo éventuel</param>
    /// <returns>Le montant final après réduction</returns>
    /// <response code="200">Réduction appliquée avec succès</response>
    /// <response code="400">Requête invalide</response>
    [HttpPost("apply")]
    [ProducesResponseType(typeof(ApplyDiscountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApplyDiscountResponse>> ApplyDiscount([FromBody] ApplyDiscountRequest request)
    {
        if (request == null || request.CartTotal <= 0)
        {
            return BadRequest("Le total du panier doit être supérieur à 0");
        }
        
        try
        {
            var response = await _discountApplicationService.ApplyDiscountsAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'application de la réduction");
            return StatusCode(500, "Une erreur est survenue lors de l'application de la réduction");
        }
    }
    
    /// <summary>
    /// Valide un code promo et retourne ses informations.
    /// </summary>
    /// <param name="code">Le code promo à valider</param>
    /// <param name="cartTotal">Montant total du panier (optionnel)</param>
    /// <returns>Informations sur la validité du code</returns>
    /// <response code="200">Code validé</response>
    /// <response code="404">Code non trouvé</response>
    [HttpGet("validate/{code}")]
    [ProducesResponseType(typeof(ValidateCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ValidateCodeResponse>> ValidateCode(
        string code,
        [FromQuery] decimal? cartTotal = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest("Le code ne peut pas être vide");
        }
        
        var discountCode = await _dbContext.Codes
            .FirstOrDefaultAsync(c => c.CodeValue == code);
        
        if (discountCode == null)
        {
            return NotFound(new ValidateCodeResponse
            {
                IsValid = false,
                ErrorMessage = "Code promo non trouvé"
            });
        }
        
        // Mettre à jour le statut selon les dates
        DiscountValidationService.UpdateCodeStatus(discountCode);
        await _dbContext.SaveChangesAsync();
        
        // Valider le code
        var validation = DiscountValidationService.ValidateCode(
            discountCode,
            cartTotal ?? 0);
        
        var response = new ValidateCodeResponse
        {
            IsValid = validation.IsValid,
            ErrorMessage = validation.IsValid ? null : validation.ErrorMessage
        };
        
        if (validation.IsValid)
        {
            response.CodeInfo = new CodeInfo
            {
                CodeValue = discountCode.CodeValue,
                Description = discountCode.Description,
                Percentage = discountCode.Percentage,
                Amount = discountCode.Amount,
                MinimumPurchaseAmount = discountCode.MinimumPurchaseAmount,
                StartDate = discountCode.StartDate,
                EndDate = discountCode.EndDate,
                Status = discountCode.Status,
                IsStackable = discountCode.IsStackable,
                MaxCumulativeDiscountPercentage = discountCode.MaxCumulativeDiscountPercentage
            };
        }
        
        return Ok(response);
    }
    
    /// <summary>
    /// Récupère les réductions applicables à un produit spécifique.
    /// </summary>
    /// <param name="productId">ID du produit</param>
    /// <returns>Les réductions disponibles pour ce produit</returns>
    /// <response code="200">Réductions trouvées</response>
    /// <response code="404">Produit non trouvé</response>
    [HttpGet("product/{productId}")]
    [ProducesResponseType(typeof(ProductDiscountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDiscountResponse>> GetProductDiscounts(Guid productId)
    {
        var coupon = await _dbContext.Coupons
            .FirstOrDefaultAsync(c => c.ProductId == productId);
        
        // Récupérer aussi par nom si nécessaire
        if (coupon == null)
        {
            // On peut chercher par d'autres critères si nécessaire
        }
        
        var response = new ProductDiscountResponse
        {
            ProductId = productId
        };
        
        if (coupon != null)
        {
            response.ProductName = coupon.ProductName;
            DiscountValidationService.UpdateCouponStatus(coupon);
            await _dbContext.SaveChangesAsync();
            
            if (coupon.Status == "Active")
            {
                response.Coupon = new CouponInfo
                {
                    Id = coupon.Id,
                    Description = coupon.Description,
                    Percentage = coupon.Percentage,
                    Amount = coupon.Amount,
                    StartDate = coupon.StartDate,
                    EndDate = coupon.EndDate,
                    Status = coupon.Status
                };
            }
        }
        
        // Récupérer les réductions automatiques
        var automaticDiscounts = await _automaticDiscountService.GetActiveAutomaticDiscountsAsync();
        
        if (coupon?.ApplicableCategories != null && coupon.ApplicableCategories.Any())
        {
            automaticDiscounts = await _automaticDiscountService
                .GetAutomaticDiscountsForCategoryAsync(coupon.ApplicableCategories.First());
        }
        
        response.AutomaticDiscounts = automaticDiscounts
            .Where(d => d.Status == "Active")
            .Select(d => new AutomaticDiscountInfo
            {
                Type = d.AutomaticType ?? "General",
                Description = d.Description,
                Percentage = d.Percentage,
                Amount = d.Amount,
                StartDate = d.StartDate,
                EndDate = d.EndDate
            })
            .ToList();
        
        return Ok(response);
    }
    
    #region Codes Promo CRUD
    
    /// <summary>
    /// Crée un nouveau code promo.
    /// </summary>
    [HttpPost("codes")]
    [ProducesResponseType(typeof(CodeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CodeResponse>> CreateCode([FromBody] CreateCodeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CodeValue))
        {
            return BadRequest("Le code ne peut pas être vide");
        }
        
        // Vérifier que le code n'existe pas déjà
        var existingCode = await _dbContext.Codes
            .FirstOrDefaultAsync(c => c.CodeValue == request.CodeValue);
        
        if (existingCode != null)
        {
            return BadRequest($"Le code {request.CodeValue} existe déjà");
        }
        
        var code = new Code
        {
            CodeValue = request.CodeValue,
            Description = request.Description,
            Amount = request.Amount,
            Percentage = request.Percentage,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MinimumPurchaseAmount = request.MinimumPurchaseAmount,
            ApplicableCategories = request.ApplicableCategories,
            IsStackable = request.IsStackable,
            MaxCumulativeDiscountPercentage = request.MaxCumulativeDiscountPercentage,
            IsAutomatic = request.IsAutomatic,
            AutomaticType = request.AutomaticType,
            TierRules = request.TierRules,
            Status = "Active"
        };
        
        DiscountValidationService.UpdateCodeStatus(code);
        
        await _dbContext.Codes.AddAsync(code);
        await _dbContext.SaveChangesAsync();
        
        var response = new CodeResponse
        {
            Id = code.Id,
            CodeValue = code.CodeValue,
            Description = code.Description,
            Amount = code.Amount,
            Percentage = code.Percentage,
            Status = code.Status,
            StartDate = code.StartDate,
            EndDate = code.EndDate,
            MinimumPurchaseAmount = code.MinimumPurchaseAmount,
            ApplicableCategories = code.ApplicableCategories,
            IsStackable = code.IsStackable,
            MaxCumulativeDiscountPercentage = code.MaxCumulativeDiscountPercentage,
            IsAutomatic = code.IsAutomatic,
            AutomaticType = code.AutomaticType,
            TierRules = code.TierRules
        };
        
        return CreatedAtAction(nameof(GetCode), new { id = code.Id }, response);
    }
    
    /// <summary>
    /// Liste tous les codes promo avec filtres optionnels.
    /// </summary>
    [HttpGet("codes")]
    [ProducesResponseType(typeof(List<CodeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CodeResponse>>> GetCodes(
        [FromQuery] string? status = null,
        [FromQuery] bool? isAutomatic = null)
    {
        var query = _dbContext.Codes.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(c => c.Status == status);
        }
        
        if (isAutomatic.HasValue)
        {
            query = query.Where(c => c.IsAutomatic == isAutomatic.Value);
        }
        
        var codes = await query.ToListAsync();
        
        // Mettre à jour les statuts selon les dates
        foreach (var code in codes)
        {
            DiscountValidationService.UpdateCodeStatus(code);
        }
        await _dbContext.SaveChangesAsync();
        
        var response = codes.Select(c => new CodeResponse
        {
            Id = c.Id,
            CodeValue = c.CodeValue,
            Description = c.Description,
            Amount = c.Amount,
            Percentage = c.Percentage,
            Status = c.Status,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            MinimumPurchaseAmount = c.MinimumPurchaseAmount,
            ApplicableCategories = c.ApplicableCategories,
            IsStackable = c.IsStackable,
            MaxCumulativeDiscountPercentage = c.MaxCumulativeDiscountPercentage,
            IsAutomatic = c.IsAutomatic,
            AutomaticType = c.AutomaticType,
            TierRules = c.TierRules
        }).ToList();
        
        return Ok(response);
    }
    
    /// <summary>
    /// Récupère un code promo par son ID.
    /// </summary>
    [HttpGet("codes/{id}")]
    [ProducesResponseType(typeof(CodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CodeResponse>> GetCode(int id)
    {
        var code = await _dbContext.Codes.FindAsync(id);
        
        if (code == null)
        {
            return NotFound($"Code avec l'ID {id} non trouvé");
        }
        
        DiscountValidationService.UpdateCodeStatus(code);
        await _dbContext.SaveChangesAsync();
        
        var response = new CodeResponse
        {
            Id = code.Id,
            CodeValue = code.CodeValue,
            Description = code.Description,
            Amount = code.Amount,
            Percentage = code.Percentage,
            Status = code.Status,
            StartDate = code.StartDate,
            EndDate = code.EndDate,
            MinimumPurchaseAmount = code.MinimumPurchaseAmount,
            ApplicableCategories = code.ApplicableCategories,
            IsStackable = code.IsStackable,
            MaxCumulativeDiscountPercentage = code.MaxCumulativeDiscountPercentage,
            IsAutomatic = code.IsAutomatic,
            AutomaticType = code.AutomaticType,
            TierRules = code.TierRules
        };
        
        return Ok(response);
    }
    
    /// <summary>
    /// Met à jour un code promo existant.
    /// </summary>
    [HttpPut("codes/{id}")]
    [ProducesResponseType(typeof(CodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CodeResponse>> UpdateCode(int id, [FromBody] UpdateCodeRequest request)
    {
        var code = await _dbContext.Codes.FindAsync(id);
        
        if (code == null)
        {
            return NotFound($"Code avec l'ID {id} non trouvé");
        }
        
        // Vérifier que le nouveau code value n'existe pas déjà (si changé)
        if (request.CodeValue != code.CodeValue)
        {
            var existingCode = await _dbContext.Codes
                .FirstOrDefaultAsync(c => c.CodeValue == request.CodeValue);
            
            if (existingCode != null)
            {
                return BadRequest($"Le code {request.CodeValue} existe déjà");
            }
        }
        
        code.CodeValue = request.CodeValue;
        code.Description = request.Description;
        code.Amount = request.Amount;
        code.Percentage = request.Percentage;
        code.StartDate = request.StartDate;
        code.EndDate = request.EndDate;
        code.MinimumPurchaseAmount = request.MinimumPurchaseAmount;
        code.ApplicableCategories = request.ApplicableCategories;
        code.IsStackable = request.IsStackable;
        code.MaxCumulativeDiscountPercentage = request.MaxCumulativeDiscountPercentage;
        code.IsAutomatic = request.IsAutomatic;
        code.AutomaticType = request.AutomaticType;
        code.TierRules = request.TierRules;
        
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            code.Status = request.Status;
        }
        
        DiscountValidationService.UpdateCodeStatus(code);
        
        _dbContext.Codes.Update(code);
        await _dbContext.SaveChangesAsync();
        
        var response = new CodeResponse
        {
            Id = code.Id,
            CodeValue = code.CodeValue,
            Description = code.Description,
            Amount = code.Amount,
            Percentage = code.Percentage,
            Status = code.Status,
            StartDate = code.StartDate,
            EndDate = code.EndDate,
            MinimumPurchaseAmount = code.MinimumPurchaseAmount,
            ApplicableCategories = code.ApplicableCategories,
            IsStackable = code.IsStackable,
            MaxCumulativeDiscountPercentage = code.MaxCumulativeDiscountPercentage,
            IsAutomatic = code.IsAutomatic,
            AutomaticType = code.AutomaticType,
            TierRules = code.TierRules
        };
        
        return Ok(response);
    }
    
    /// <summary>
    /// Supprime un code promo.
    /// </summary>
    [HttpDelete("codes/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCode(int id)
    {
        var code = await _dbContext.Codes.FindAsync(id);
        
        if (code == null)
        {
            return NotFound($"Code avec l'ID {id} non trouvé");
        }
        
        _dbContext.Codes.Remove(code);
        await _dbContext.SaveChangesAsync();
        
        return NoContent();
    }
    
    #endregion
    
    #region Coupons CRUD
    
    /// <summary>
    /// Crée un nouveau coupon.
    /// </summary>
    [HttpPost("coupons")]
    [ProducesResponseType(typeof(CouponResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CouponResponse>> CreateCoupon([FromBody] CreateCouponRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductName))
        {
            return BadRequest("Le nom du produit ne peut pas être vide");
        }
        
        var coupon = new Coupon
        {
            ProductName = request.ProductName,
            ProductId = request.ProductId,
            Description = request.Description,
            Amount = request.Amount,
            Percentage = request.Percentage,
            ApplicableCategories = request.ApplicableCategories,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MinimumPurchaseAmount = request.MinimumPurchaseAmount,
            Status = "Active"
        };
        
        DiscountValidationService.UpdateCouponStatus(coupon);
        
        await _dbContext.Coupons.AddAsync(coupon);
        await _dbContext.SaveChangesAsync();
        
        var response = new CouponResponse
        {
            Id = coupon.Id,
            ProductName = coupon.ProductName,
            ProductId = coupon.ProductId,
            Description = coupon.Description,
            Amount = coupon.Amount,
            Percentage = coupon.Percentage,
            ApplicableCategories = coupon.ApplicableCategories,
            Status = coupon.Status,
            StartDate = coupon.StartDate,
            EndDate = coupon.EndDate,
            MinimumPurchaseAmount = coupon.MinimumPurchaseAmount
        };
        
        return CreatedAtAction(nameof(GetCoupon), new { id = coupon.Id }, response);
    }
    
    /// <summary>
    /// Liste tous les coupons avec filtres optionnels.
    /// </summary>
    [HttpGet("coupons")]
    [ProducesResponseType(typeof(List<CouponResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CouponResponse>>> GetCoupons(
        [FromQuery] string? status = null,
        [FromQuery] string? productName = null)
    {
        var query = _dbContext.Coupons.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(c => c.Status == status);
        }
        
        if (!string.IsNullOrWhiteSpace(productName))
        {
            query = query.Where(c => c.ProductName.Contains(productName));
        }
        
        var coupons = await query.ToListAsync();
        
        // Mettre à jour les statuts selon les dates
        foreach (var coupon in coupons)
        {
            DiscountValidationService.UpdateCouponStatus(coupon);
        }
        await _dbContext.SaveChangesAsync();
        
        var response = coupons.Select(c => new CouponResponse
        {
            Id = c.Id,
            ProductName = c.ProductName,
            ProductId = c.ProductId,
            Description = c.Description,
            Amount = c.Amount,
            Percentage = c.Percentage,
            ApplicableCategories = c.ApplicableCategories,
            Status = c.Status,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            MinimumPurchaseAmount = c.MinimumPurchaseAmount
        }).ToList();
        
        return Ok(response);
    }
    
    /// <summary>
    /// Récupère un coupon par son ID.
    /// </summary>
    [HttpGet("coupons/{id}")]
    [ProducesResponseType(typeof(CouponResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CouponResponse>> GetCoupon(int id)
    {
        var coupon = await _dbContext.Coupons.FindAsync(id);
        
        if (coupon == null)
        {
            return NotFound($"Coupon avec l'ID {id} non trouvé");
        }
        
        DiscountValidationService.UpdateCouponStatus(coupon);
        await _dbContext.SaveChangesAsync();
        
        var response = new CouponResponse
        {
            Id = coupon.Id,
            ProductName = coupon.ProductName,
            ProductId = coupon.ProductId,
            Description = coupon.Description,
            Amount = coupon.Amount,
            Percentage = coupon.Percentage,
            ApplicableCategories = coupon.ApplicableCategories,
            Status = coupon.Status,
            StartDate = coupon.StartDate,
            EndDate = coupon.EndDate,
            MinimumPurchaseAmount = coupon.MinimumPurchaseAmount
        };
        
        return Ok(response);
    }
    
    /// <summary>
    /// Met à jour un coupon existant.
    /// </summary>
    [HttpPut("coupons/{id}")]
    [ProducesResponseType(typeof(CouponResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CouponResponse>> UpdateCoupon(int id, [FromBody] UpdateCouponRequest request)
    {
        var coupon = await _dbContext.Coupons.FindAsync(id);
        
        if (coupon == null)
        {
            return NotFound($"Coupon avec l'ID {id} non trouvé");
        }
        
        coupon.ProductName = request.ProductName;
        coupon.ProductId = request.ProductId;
        coupon.Description = request.Description;
        coupon.Amount = request.Amount;
        coupon.Percentage = request.Percentage;
        coupon.ApplicableCategories = request.ApplicableCategories;
        coupon.StartDate = request.StartDate;
        coupon.EndDate = request.EndDate;
        coupon.MinimumPurchaseAmount = request.MinimumPurchaseAmount;
        
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            coupon.Status = request.Status;
        }
        
        DiscountValidationService.UpdateCouponStatus(coupon);
        
        _dbContext.Coupons.Update(coupon);
        await _dbContext.SaveChangesAsync();
        
        var response = new CouponResponse
        {
            Id = coupon.Id,
            ProductName = coupon.ProductName,
            ProductId = coupon.ProductId,
            Description = coupon.Description,
            Amount = coupon.Amount,
            Percentage = coupon.Percentage,
            ApplicableCategories = coupon.ApplicableCategories,
            Status = coupon.Status,
            StartDate = coupon.StartDate,
            EndDate = coupon.EndDate,
            MinimumPurchaseAmount = coupon.MinimumPurchaseAmount
        };
        
        return Ok(response);
    }
    
    /// <summary>
    /// Supprime un coupon.
    /// </summary>
    [HttpDelete("coupons/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCoupon(int id)
    {
        var coupon = await _dbContext.Coupons.FindAsync(id);
        
        if (coupon == null)
        {
            return NotFound($"Coupon avec l'ID {id} non trouvé");
        }
        
        _dbContext.Coupons.Remove(coupon);
        await _dbContext.SaveChangesAsync();
        
        return NoContent();
    }
    
    #endregion
}

