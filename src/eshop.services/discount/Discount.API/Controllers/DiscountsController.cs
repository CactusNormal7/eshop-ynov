using Discount.API.DTOs;
using Discount.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Discount.API.Controllers;

/// <summary>
/// Contrôleur pour gérer les coupons de réduction (CRUD)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DiscountsController : ControllerBase
{
    private readonly IDiscountService _discountService;
    private readonly ILogger<DiscountsController> _logger;

    public DiscountsController(
        IDiscountService discountService,
        ILogger<DiscountsController> logger)
    {
        _discountService = discountService;
        _logger = logger;
    }

    /// <summary>
    /// Récupère un coupon par nom de produit
    /// </summary>
    /// <param name="productName">Nom du produit</param>
    /// <returns>Le coupon associé au produit</returns>
    [HttpGet("product/{productName}")]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CouponDto>> GetDiscountByProduct(string productName)
    {
        var coupon = await _discountService.GetDiscountByProductNameAsync(productName);
        
        if (coupon == null)
            return NotFound($"Aucun coupon trouvé pour le produit {productName}");
            
        return Ok(coupon);
    }

    /// <summary>
    /// Récupère un coupon par code promo
    /// </summary>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CouponDto>> GetDiscountByCode(string code)
    {
        var coupon = await _discountService.GetDiscountByCodeAsync(code);
        
        if (coupon == null)
            return NotFound($"Aucun coupon trouvé pour le code {code}");
            
        return Ok(coupon);
    }

    /// <summary>
    /// Récupère tous les coupons d'une catégorie
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(List<CouponDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CouponDto>>> GetDiscountsByCategory(
        string category,
        [FromQuery] bool includeInactive = false)
    {
        var coupons = await _discountService.GetDiscountsByCategoryAsync(category, includeInactive);
        return Ok(coupons);
    }

    /// <summary>
    /// Crée un nouveau coupon de réduction
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CouponDto>> CreateDiscount([FromBody] CreateCouponDto dto)
    {
        try
        {
            var coupon = await _discountService.CreateDiscountAsync(dto);
            return CreatedAtAction(
                nameof(GetDiscountByProduct),
                new { productName = coupon.ProductName },
                coupon);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du coupon");
            return BadRequest($"Erreur lors de la création du coupon: {ex.Message}");
        }
    }

    /// <summary>
    /// Met à jour un coupon existant
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CouponDto>> UpdateDiscount(int id, [FromBody] UpdateCouponDto dto)
    {
        if (id != dto.Id)
            return BadRequest("L'ID dans l'URL ne correspond pas à l'ID dans le corps de la requête");
            
        try
        {
            var coupon = await _discountService.UpdateDiscountAsync(dto);
            return Ok(coupon);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du coupon {Id}", id);
            return NotFound($"Coupon {id} non trouvé");
        }
    }

    /// <summary>
    /// Supprime un coupon
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDiscount(int id, [FromQuery] string? productName = null)
    {
        var success = await _discountService.DeleteDiscountAsync(id, productName);
        
        if (!success)
            return NotFound($"Coupon {id} non trouvé");
            
        return NoContent();
    }
}

/// <summary>
/// Contrôleur pour les opérations métier sur les réductions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DiscountOperationsController : ControllerBase
{
    private readonly IDiscountService _discountService;
    private readonly ILogger<DiscountOperationsController> _logger;

    public DiscountOperationsController(
        IDiscountService discountService,
        ILogger<DiscountOperationsController> logger)
    {
        _discountService = discountService;
        _logger = logger;
    }

    /// <summary>
    /// Valide un code promo pour un panier
    /// </summary>
    /// <remarks>
    /// Exemple de requête:
    /// 
    ///     POST /api/discount/validate
    ///     {
    ///         "code": "SUMMER2024",
    ///         "cartTotal": 150.00
    ///     }
    /// </remarks>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidateCouponCodeResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ValidateCouponCodeResponseDto>> ValidateCouponCode(
        [FromBody] ValidateCouponCodeDto dto)
    {
        var result = await _discountService.ValidateCouponCodeAsync(dto.Code, dto.CartTotal);
        return Ok(result);
    }

    /// <summary>
    /// Applique une réduction à un produit
    /// </summary>
    /// <remarks>
    /// Exemple de requête:
    /// 
    ///     POST /api/discount/apply
    ///     {
    ///         "productName": "IPhone X",
    ///         "originalPrice": 999.99,
    ///         "discountCode": "SUMMER2024",
    ///         "cartTotal": 1500.00
    ///     }
    /// </remarks>
    [HttpPost("apply")]
    [ProducesResponseType(typeof(ApplyDiscountResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApplyDiscountResponseDto>> ApplyDiscount([FromBody] ApplyDiscountDto dto)
    {
        var result = await _discountService.ApplyDiscountAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Calcule la réduction totale pour un panier
    /// </summary>
    /// <remarks>
    /// Exemple de requête:
    /// 
    ///     POST /api/discount/calculate-total
    ///     {
    ///         "items": [
    ///             {
    ///                 "productName": "IPhone X",
    ///                 "price": 999.99,
    ///                 "quantity": 1,
    ///                 "category": "Electronics"
    ///             }
    ///         ],
    ///         "discountCode": "SUMMER2024",
    ///         "cartTotal": 1500.00
    ///     }
    /// </remarks>
    [HttpPost("calculate-total")]
    [ProducesResponseType(typeof(CalculateTotalDiscountResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CalculateTotalDiscountResponseDto>> CalculateTotalDiscount(
        [FromBody] CalculateTotalDiscountDto dto)
    {
        var result = await _discountService.CalculateTotalDiscountAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Récupère les réductions applicables à un produit
    /// </summary>
    [HttpGet("product/{productId}/applicable")]
    [ProducesResponseType(typeof(List<CouponDto>), StatusCodes.Status200OK)]
    [Obsolete("Utiliser GET /api/discounts/product/{productName} à la place")]
    public async Task<ActionResult<List<CouponDto>>> GetApplicableDiscounts(string productId)
    {
        // Pour compatibilité, recherche par nom de produit
        var coupon = await _discountService.GetDiscountByProductNameAsync(productId);
        
        if (coupon == null)
            return Ok(new List<CouponDto>());
            
        return Ok(new List<CouponDto> { coupon });
    }
}

