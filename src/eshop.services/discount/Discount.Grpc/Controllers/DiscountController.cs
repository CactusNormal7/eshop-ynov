using Microsoft.AspNetCore.Mvc;
using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Controllers;

[ApiController]
[Route("discounts")]
public class DiscountController : ControllerBase
{
    private readonly DiscountContext _context;
    private readonly ILogger<DiscountController> _logger;

    public DiscountController(DiscountContext context, ILogger<DiscountController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // POST /discounts/apply
    [HttpPost("apply")]
    public async Task<IActionResult> ApplyDiscount([FromBody] Coupon coupon)
    {
        // Appliquer ou ajouter un coupon (même logique que CreateDiscount)
        await _context.Coupons.AddAsync(coupon);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Coupon appliqué : {ProductName}, {Amount}", coupon.ProductName, coupon.Amount);
        return Ok(coupon);
    }

    // GET /discounts/validate/{code}
    [HttpGet("validate/{code}")]
    public async Task<IActionResult> ValidateCode(string code)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.ProductName == code);
        if (coupon == null)
            return NotFound(new { valid = false });
        return Ok(new { valid = true, coupon });
    }

    // GET /discounts/product/{productId}
    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetDiscountsForProduct(string productId)
    {
        var coupons = await _context.Coupons.Where(c => c.ProductName == productId).ToListAsync();
        return Ok(coupons);
    }
}
