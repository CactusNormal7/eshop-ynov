using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Grpc.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Services;

/// <summary>
/// The DiscountServiceServer class implements the gRPC service for managing discount data.
/// It provides CRUD operations for discounts and communicates with the underlying database using a DbContext.
/// This class inherits from DiscountProtoServiceBase, which defines the service methods in the gRPC contract,
/// and implements the necessary logic for handling those methods.
/// </summary>
/// <remarks>
/// This class uses the DiscountContext for database interactions and ILogger for logging purposes.
/// It is registered with the gRPC pipeline in the application startup configuration.
/// </remarks>
public class DiscountServiceServer(DiscountContext dbContext, ILogger<DiscountServiceServer> logger) : DiscountProtoService.DiscountProtoServiceBase
{
    /// <summary>
    /// Retrieves discount details for a given product from the database.
    /// </summary>
    /// <param name="request">The request containing the product name to fetch the discount for.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>
    /// Returns a <see cref="CouponModel"/> containing the discount details for the specified product.
    /// </returns>
    /// <exception cref="RpcException">
    /// Thrown if no discount is found for the specified product name.
    /// </exception>
    public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        logger.LogInformation("Retrieving discount for {ProductName}", request.ProductName);
        
        var coupon = await dbContext.Coupons
            .FirstOrDefaultAsync(x => x.ProductName == request.ProductName && 
                                     (x.Status == Models.CouponStatus.Active || x.Status == Models.CouponStatus.Upcoming));
        
        if (coupon == null)
        {
            // Retourner un coupon avec amount=0 si non trouvé (selon règles métier)
            return new CouponModel
            {
                ProductName = request.ProductName,
                Amount = 0,
                Description = "No Discount",
                DiscountType = (DiscountProtoService.DiscountType)(int)Models.DiscountType.FixedAmount,
                Status = (DiscountProtoService.CouponStatus)(int)Models.CouponStatus.Active
            };
        }
        
        // Mettre à jour le statut en fonction des dates
        coupon.Status = DiscountCalculationService.UpdateCouponStatus(coupon);
        
        logger.LogInformation("Discount retrieved for {ProductName}: {Amount}", coupon.ProductName, coupon.Amount);
        
        return coupon.ToCouponModel();
    }
    
    /// <summary>
    /// Récupère un coupon par son code promo
    /// </summary>
    public override async Task<CouponModel> GetDiscountByCode(GetDiscountByCodeRequest request, ServerCallContext context)
    {
        logger.LogInformation("Retrieving discount for code {Code}", request.Code);
        
        var coupon = await dbContext.Coupons
            .FirstOrDefaultAsync(x => x.Code == request.Code);
        
        if (coupon == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Coupon with code {request.Code} not found"));
        
        coupon.Status = DiscountCalculationService.UpdateCouponStatus(coupon);
        
        return coupon.ToCouponModel();
    }
    
    /// <summary>
    /// Récupère les coupons par catégorie
    /// </summary>
    public override async Task<CouponListResponse> GetDiscountsByCategory(GetDiscountsByCategoryRequest request, ServerCallContext context)
    {
        logger.LogInformation("Retrieving discounts for category {Category}", request.Category);
        
        var query = dbContext.Coupons
            .Where(x => x.Category == request.Category);
        
        if (!request.IncludeInactive)
        {
            query = query.Where(x => x.Status == CouponStatus.Active || x.Status == CouponStatus.Upcoming);
        }
        
        var coupons = await query.ToListAsync();
        
        // Mettre à jour les statuts
        foreach (var coupon in coupons)
        {
            coupon.Status = DiscountCalculationService.UpdateCouponStatus(coupon);
        }
        
        var response = new CouponListResponse();
        response.Coupons.AddRange(coupons.Select(c => c.ToCouponModel()));
        
        return response;
    }

    /// <summary>
    /// Creates a new discount for a specified product and stores it in the database.
    /// </summary>
    /// <param name="request">The request containing the details of the new discount to create, including the coupon information.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>
    /// Returns a <see cref="CouponModel"/> representing the newly created discount.
    /// </returns>
    /// <exception cref="RpcException">
    /// Thrown if the request's coupon information is null.
    /// </exception>
    public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
    {
        if (request.Coupon is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Coupon is null"));
        
        var coupon = request.Coupon.ToCoupon();
        
        // Vérifier si un code existe déjà
        if (!string.IsNullOrWhiteSpace(coupon.Code))
        {
            var existingCode = await dbContext.Coupons.FirstOrDefaultAsync(x => x.Code == coupon.Code);
            if (existingCode != null)
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"Un coupon avec le code {coupon.Code} existe déjà"));
        }
        
        coupon.CreatedAt = DateTime.UtcNow;
        coupon.Status = DiscountCalculationService.UpdateCouponStatus(coupon);
        
        logger.LogInformation("Creating new discount for {ProductName}", coupon.ProductName ?? "Global");
        await dbContext.Coupons.AddAsync(coupon);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Discount created for {ProductName}: {Amount}", coupon.ProductName ?? "Global", coupon.Amount);
        return coupon.ToCouponModel();
    }

    /// <summary>
    /// Updates the discount details for a specific product based on the provided request.
    /// </summary>
    /// <param name="request">An object containing the updated discount information for a specific product.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>
    /// Returns an updated <see cref="CouponModel"/> containing the modified discount details.
    /// </returns>
    /// <exception cref="RpcException">
    /// Thrown if the provided coupon is null, or if the specified product or coupon identifier is not found in the database.
    /// </exception>
    public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
    {
        if (request.Coupon is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Coupon is null"));
        
        logger.LogInformation("Updating discount for {ProductName}", request.Coupon.ProductName);

        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(x => x.ProductName == request.Coupon.ProductName 
                                                                      || x.Id == request.Coupon.Id);
        if(coupon is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Coupon with name {request.Coupon.ProductName} " +
                                                                   $" or Id {request.Coupon.Id} not found"));
        // Mapper les propriétés
        var excessiveCoupon = request.Coupon.ToCoupon();
        coupon.ProductName = excessiveCoupon.ProductName;
        coupon.Category = excessiveCoupon.Category;
        coupon.Description = excessiveCoupon.Description;
        coupon.Code = excessiveCoupon.Code;
        coupon.DiscountType = excessiveCoupon.DiscountType;
        coupon.Amount = excessiveCoupon.Amount;
        coupon.Percentage = excessiveCoupon.Percentage;
        coupon.MinimumAmount = excessiveCoupon.MinimumAmount;
        coupon.StartDate = excessiveCoupon.StartDate;
        coupon.EndDate = excessiveCoupon.EndDate;
        coupon.Status = DiscountCalculationService.UpdateCouponStatus(excessiveCoupon);
        coupon.IsStackable = excessiveCoupon.IsStackable;
        coupon.MaxStackablePercentage = excessiveCoupon.MaxStackablePercentage;
        coupon.RemainingUses = excessiveCoupon.RemainingUses;
        coupon.IsAutomatic = excessiveCoupon.IsAutomatic;
        coupon.UpdatedAt = DateTime.UtcNow;
        
        dbContext.Coupons.Update(coupon);
        await dbContext.SaveChangesAsync();
        
        logger.LogInformation("Discount updated for {ProductName}: {Amount}", coupon.ProductName ?? "Global", coupon.Amount);
        return coupon.ToCouponModel();
    }

    /// <summary>
    /// Deletes a discount for a specified product based on the provided coupon details.
    /// </summary>
    /// <param name="request">The request containing the details of the coupon to be deleted, including the product name or ID.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>
    /// Returns a <see cref="DeleteDiscountResponse"/> indicating whether the discount was successfully deleted.
    /// </returns>
    /// <exception cref="RpcException">
    /// Thrown if the provided coupon is null, or if no matching discount is found for the specified product name or ID.
    /// </exception>
    public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request,
        ServerCallContext context)
    {
        if (request.Coupon is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Coupon is null"));

        logger.LogInformation("Deleting discount for {ProductName}", request.Coupon.ProductName);
        
        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(x => x.ProductName == request.Coupon.ProductName 
                                                                      || x.Id == request.Coupon.Id);
        if(coupon is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Coupon with name {request.Coupon.ProductName} " +
                                                                   $" or Id {request.Coupon.Id} not found"));
        dbContext.Coupons.Remove(coupon);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Discount deleted for {ProductName}", coupon.ProductName);
        
        return new DeleteDiscountResponse(){Success = true};
    }
    
    /// <summary>
    /// Valide un code de réduction
    /// </summary>
    public override async Task<ValidateDiscountCodeResponse> ValidateDiscountCode(
        ValidateDiscountCodeRequest request, 
        ServerCallContext context)
    {
        logger.LogInformation("Validating discount code {Code}", request.Code);
        
        var coupon = await dbContext.Coupons
            .FirstOrDefaultAsync(x => x.Code == request.Code);
        
        if (coupon == null)
        {
            return new ValidateDiscountCodeResponse
            {
                IsValid = false,
                Message = "Code promo introuvable"
            };
        }
        
        coupon.Status = DiscountCalculationService.UpdateCouponStatus(coupon);
        
        var (isValid, message) = DiscountCalculationService.ValidateCoupon(
            coupon, 
            request.CartTotal, 
            isCodeRequired: true);
        
        return new ValidateDiscountCodeResponse
        {
            IsValid = isValid,
            Message = message,
            Coupon = isValid ? coupon.ToCouponModel() : null
        };
    }
    
    /// <summary>
    /// Applique une réduction à un produit
    /// </summary>
    public override async Task<ApplyDiscountResponse> ApplyDiscount(
        ApplyDiscountRequest request, 
        ServerCallContext context)
    {
        logger.LogInformation("Applying discount for product {ProductName}", request.ProductName);
        
        var appliedCoupons = new List<Coupon>();
        var totalDiscountAmount = 0.0;
        var totalDiscountPercentage = 0.0;
        
        // Récupérer les coupons automatiques pour le produit
        var automaticCoupons = await dbContext.Coupons
            .Where(x => (x.ProductName == request.ProductName || string.IsNullOrEmpty(x.ProductName)) &&
                       x.IsAutomatic &&
                       (x.Status == Models.CouponStatus.Active || x.Status == Models.CouponStatus.Upcoming))
            .ToListAsync();
        
        // Si un code est fourni, le récupérer aussi
        Coupon? codeCoupon = null;
        if (!string.IsNullOrWhiteSpace(request.DiscountCode))
        {
            codeCoupon = await dbContext.Coupons
                .FirstOrDefaultAsync(x => x.Code == request.DiscountCode);
            
            if (codeCoupon != null)
            {
                codeCoupon.Status = DiscountCalculationService.UpdateCouponStatus(codeCoupon);
                var (isValid, _) = DiscountCalculationService.ValidateCoupon(codeCoupon, request.CartTotal, true);
                if (isValid)
                {
                    automaticCoupons.Add(codeCoupon);
                }
            }
        }
        
        // Calculer la réduction cumulée
        var (discountAmount, finalPrice, coupons) = DiscountCalculationService.CalculateCumulativeDiscount(
            automaticCoupons,
            request.OriginalPrice,
            request.CartTotal,
            maxStackablePercentage: 30.0); // Maximum 30% selon règles métier
        
        var response = new ApplyDiscountResponse
        {
            DiscountedPrice = finalPrice,
            DiscountAmount = discountAmount,
            DiscountPercentage = (discountAmount / request.OriginalPrice) * 100
        };
        
        response.AppliedCoupons.AddRange(coupons.Select(c => c.ToCouponModel()));
        
        return response;
    }
    
    /// <summary>
    /// Calcule la réduction totale pour un panier
    /// </summary>
    public override async Task<CalculateTotalDiscountResponse> CalculateTotalDiscount(
        CalculateTotalDiscountRequest request, 
        ServerCallContext context)
    {
        logger.LogInformation("Calculating total discount for cart with {ItemCount} items", request.Items.Count);
        
        var appliedCoupons = new List<Coupon>();
        var totalDiscount = 0.0;
        
        // Récupérer tous les coupons automatiques actifs
        var allProductNames = request.Items.Select(i => i.ProductName).Distinct().ToList();
        var allCategories = request.Items.SelectMany(i => !string.IsNullOrEmpty(i.Category) ? new[] { i.Category } : Array.Empty<string>()).Distinct().ToList();
        
        var availableCoupons = await dbContext.Coupons
            .Where(x => x.IsAutomatic &&
                       (x.Status == CouponStatus.Active || x.Status == CouponStatus.Upcoming) &&
                       (string.IsNullOrEmpty(x.ProductName) || allProductNames.Contains(x.ProductName) ||
                        (!string.IsNullOrEmpty(x.Category) && allCategories.Contains(x.Category))))
            .ToListAsync();
        
        // Si un code est fourni, l'ajouter
        if (!string.IsNullOrWhiteSpace(request.DiscountCode))
        {
            var codeCoupon = await dbContext.Coupons
                .FirstOrDefaultAsync(x => x.Code == request.DiscountCode);
            
            if (codeCoupon != null)
            {
                codeCoupon.Status = DiscountCalculationService.UpdateCouponStatus(codeCoupon);
                var (isValid, _) = DiscountCalculationService.ValidateCoupon(codeCoupon, request.CartTotal, true);
                if (isValid)
                {
                    availableCoupons.Add(codeCoupon);
                }
            }
        }
        
        // Appliquer les réductions pour chaque item
        foreach (var item in request.Items)
        {
            var itemCoupons = availableCoupons
                .Where(c => string.IsNullOrEmpty(c.ProductName) || c.ProductName == item.ProductName ||
                           (!string.IsNullOrEmpty(c.Category) && c.Category == item.Category))
                .ToList();
            
            if (itemCoupons.Any())
            {
                var (discountAmount, _, coupons) = DiscountCalculationService.CalculateCumulativeDiscount(
                    itemCoupons,
                    item.Price,
                    request.CartTotal,
                    maxStackablePercentage: 30.0);
                
                totalDiscount += discountAmount * item.Quantity;
                appliedCoupons.AddRange(coupons.Where(c => !appliedCoupons.Any(ac => ac.Id == c.Id)));
            }
        }
        
        var finalTotal = request.CartTotal - totalDiscount;
        
        var response = new CalculateTotalDiscountResponse
        {
            TotalDiscount = totalDiscount,
            FinalTotal = Math.Max(0, finalTotal)
        };
        
        response.AppliedCoupons.AddRange(appliedCoupons.DistinctBy(c => c.Id).Select(c => c.ToCouponModel()));
        
        return response;
    }
}