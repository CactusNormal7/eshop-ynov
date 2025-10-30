using Discount.Grpc;
using Microsoft.AspNetCore.Mvc;

namespace Discount.API.Controllers;

[ApiController]
[Route("discounts")]
public class DiscountsController : ControllerBase
{
    private readonly DiscountProtoService.DiscountProtoServiceClient _discountClient;

    public DiscountsController(DiscountProtoService.DiscountProtoServiceClient discountClient)
    {
        _discountClient = discountClient;
    }

    [HttpPost("apply")]
    public async Task<ActionResult<ApplyDiscountResponse>> Apply([FromBody] ApplyDiscountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductName))
            return BadRequest("ProductName requis");

        if (request.OriginalPrice < 0)
            return BadRequest("OriginalPrice invalide");

        try
        {
            var coupon = await _discountClient.GetDiscountAsync(new GetDiscountRequest { ProductName = request.ProductName });

            var discountedPrice = Math.Max(0, request.OriginalPrice - (decimal)coupon.Amount);

            return Ok(new ApplyDiscountResponse
            {
                ProductName = coupon.ProductName,
                Description = coupon.Description,
                DiscountAmount = (decimal)coupon.Amount,
                OriginalPrice = request.OriginalPrice,
                DiscountedPrice = discountedPrice
            });
        }
        catch
        {
            // Si pas de coupon, on retourne le prix d'origine sans réduction
            return Ok(new ApplyDiscountResponse
            {
                ProductName = request.ProductName,
                Description = null,
                DiscountAmount = 0,
                OriginalPrice = request.OriginalPrice,
                DiscountedPrice = request.OriginalPrice
            });
        }
    }

    [HttpGet("validate/{code}")]
    public async Task<ActionResult<CouponDto>> Validate(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Code requis");

        try
        {
            var coupon = await _discountClient.GetDiscountAsync(new GetDiscountRequest { ProductName = code });
            return Ok(new CouponDto
            {
                Id = coupon.Id,
                ProductName = coupon.ProductName,
                Description = coupon.Description,
                Amount = (decimal)coupon.Amount
            });
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<CouponDto>> GetForProduct(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            return BadRequest("productId requis");

        try
        {
            var coupon = await _discountClient.GetDiscountAsync(new GetDiscountRequest { ProductName = productId });
            return Ok(new CouponDto
            {
                Id = coupon.Id,
                ProductName = coupon.ProductName,
                Description = coupon.Description,
                Amount = (decimal)coupon.Amount
            });
        }
        catch
        {
            return NotFound();
        }
    }
}

public class ApplyDiscountRequest
{
    public string ProductName { get; set; } = string.Empty;
    public decimal OriginalPrice { get; set; }
}

public class ApplyDiscountResponse
{
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountedPrice { get; set; }
}

public class CouponDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
}


