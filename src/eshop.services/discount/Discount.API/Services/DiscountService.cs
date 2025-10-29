using Discount.API.DTOs;
using Discount.Grpc;
using Grpc.Net.Client;

namespace Discount.API.Services;

/// <summary>
/// Service pour communiquer avec le service Discount.Grpc via gRPC
/// </summary>
public interface IDiscountService
{
    Task<CouponDto?> GetDiscountByProductNameAsync(string productName);
    Task<CouponDto?> GetDiscountByCodeAsync(string code);
    Task<List<CouponDto>> GetDiscountsByCategoryAsync(string category, bool includeInactive = false);
    Task<CouponDto> CreateDiscountAsync(CreateCouponDto dto);
    Task<CouponDto> UpdateDiscountAsync(UpdateCouponDto dto);
    Task<bool> DeleteDiscountAsync(int id, string? productName = null);
    Task<ValidateCouponCodeResponseDto> ValidateCouponCodeAsync(string code, double cartTotal);
    Task<ApplyDiscountResponseDto> ApplyDiscountAsync(ApplyDiscountDto dto);
    Task<CalculateTotalDiscountResponseDto> CalculateTotalDiscountAsync(CalculateTotalDiscountDto dto);
}

public class DiscountService : IDiscountService
{
    private readonly DiscountProtoService.DiscountProtoServiceClient _grpcClient;
    private readonly ILogger<DiscountService> _logger;

    public DiscountService(
        DiscountProtoService.DiscountProtoServiceClient grpcClient,
        ILogger<DiscountService> logger)
    {
        _grpcClient = grpcClient;
        _logger = logger;
    }

    public async Task<CouponDto?> GetDiscountByProductNameAsync(string productName)
    {
        try
        {
            var request = new GetDiscountRequest { ProductName = productName };
            var response = await _grpcClient.GetDiscountAsync(request);
            
            if (response.Amount == 0 && response.Description == "No Discount")
                return null;
                
            return MapToCouponDto(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du discount pour {ProductName}", productName);
            return null;
        }
    }

    public async Task<CouponDto?> GetDiscountByCodeAsync(string code)
    {
        try
        {
            var request = new GetDiscountByCodeRequest { Code = code };
            var response = await _grpcClient.GetDiscountByCodeAsync(request);
            return MapToCouponDto(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du discount pour le code {Code}", code);
            return null;
        }
    }

    public async Task<List<CouponDto>> GetDiscountsByCategoryAsync(string category, bool includeInactive = false)
    {
        try
        {
            var request = new GetDiscountsByCategoryRequest 
            { 
                Category = category, 
                IncludeInactive = includeInactive 
            };
            var response = await _grpcClient.GetDiscountsByCategoryAsync(request);
            
            return response.Coupons.Select(MapToCouponDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des discounts pour la catégorie {Category}", category);
            return new List<CouponDto>();
        }
    }

    public async Task<CouponDto> CreateDiscountAsync(CreateCouponDto dto)
    {
        var couponModel = MapToCouponModel(dto);
        var request = new CreateDiscountRequest { Coupon = couponModel };
        var response = await _grpcClient.CreateDiscountAsync(request);
        return MapToCouponDto(response);
    }

    public async Task<CouponDto> UpdateDiscountAsync(UpdateCouponDto dto)
    {
        var couponModel = MapToCouponModel(dto);
        var request = new UpdateDiscountRequest { Coupon = couponModel };
        var response = await _grpcClient.UpdateDiscountAsync(request);
        return MapToCouponDto(response);
    }

    public async Task<bool> DeleteDiscountAsync(int id, string? productName = null)
    {
        try
        {
            var couponModel = new CouponModel { Id = id };
            if (!string.IsNullOrEmpty(productName))
                couponModel.ProductName = productName;
                
            var request = new DeleteDiscountRequest { Coupon = couponModel };
            var response = await _grpcClient.DeleteDiscountAsync(request);
            return response.Success;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ValidateCouponCodeResponseDto> ValidateCouponCodeAsync(string code, double cartTotal)
    {
        try
        {
            var request = new ValidateDiscountCodeRequest 
            { 
                Code = code, 
                CartTotal = cartTotal 
            };
            var response = await _grpcClient.ValidateDiscountCodeAsync(request);
            
            return new ValidateCouponCodeResponseDto
            {
                IsValid = response.IsValid,
                Message = response.Message,
                Coupon = response.IsValid && response.Coupon != null 
                    ? MapToCouponDto(response.Coupon) 
                    : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la validation du code {Code}", code);
            return new ValidateCouponCodeResponseDto
            {
                IsValid = false,
                Message = "Erreur lors de la validation du code"
            };
        }
    }

    public async Task<ApplyDiscountResponseDto> ApplyDiscountAsync(ApplyDiscountDto dto)
    {
        try
        {
            var request = new ApplyDiscountRequest
            {
                ProductName = dto.ProductName,
                OriginalPrice = dto.OriginalPrice,
                DiscountCode = dto.DiscountCode ?? string.Empty,
                CartTotal = dto.CartTotal
            };
            
            var response = await _grpcClient.ApplyDiscountAsync(request);
            
            return new ApplyDiscountResponseDto
            {
                DiscountedPrice = response.DiscountedPrice,
                DiscountAmount = response.DiscountAmount,
                DiscountPercentage = response.DiscountPercentage,
                AppliedCoupons = response.AppliedCoupons.Select(MapToCouponDto).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'application du discount");
            return new ApplyDiscountResponseDto
            {
                DiscountedPrice = dto.OriginalPrice,
                DiscountAmount = 0,
                DiscountPercentage = 0
            };
        }
    }

    public async Task<CalculateTotalDiscountResponseDto> CalculateTotalDiscountAsync(CalculateTotalDiscountDto dto)
    {
        try
        {
            var request = new CalculateTotalDiscountRequest
            {
                DiscountCode = dto.DiscountCode ?? string.Empty,
                CartTotal = dto.CartTotal
            };
            
            request.Items.AddRange(dto.Items.Select(item => new CartItem
            {
                ProductName = item.ProductName,
                Price = item.Price,
                Quantity = item.Quantity,
                Category = item.Category ?? string.Empty
            }));
            
            var response = await _grpcClient.CalculateTotalDiscountAsync(request);
            
            return new CalculateTotalDiscountResponseDto
            {
                TotalDiscount = response.TotalDiscount,
                FinalTotal = response.FinalTotal,
                AppliedCoupons = response.AppliedCoupons.Select(MapToCouponDto).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul de la réduction totale");
            return new CalculateTotalDiscountResponseDto
            {
                TotalDiscount = 0,
                FinalTotal = dto.CartTotal
            };
        }
    }

    private CouponModel MapToCouponModel(CreateCouponDto dto)
    {
        var coupon = new CouponModel
        {
            ProductName = dto.ProductName ?? string.Empty,
            Category = dto.Category ?? string.Empty,
            Description = dto.Description,
            Code = dto.Code ?? string.Empty,
            Amount = dto.Amount,
            Percentage = dto.Percentage ?? 0,
            MinimumAmount = dto.MinimumAmount ?? 0,
            IsStackable = dto.IsStackable,
            MaxStackablePercentage = dto.MaxStackablePercentage ?? 0,
            RemainingUses = dto.RemainingUses,
            IsAutomatic = dto.IsAutomatic
        };
        
        // Map discount type
        coupon.DiscountType = Enum.Parse<Discount.Grpc.DiscountType>(dto.DiscountType, true);
        
        // Map dates
        if (dto.StartDate.HasValue)
            coupon.StartDate = new DateTimeOffset(dto.StartDate.Value).ToUnixTimeSeconds();
        if (dto.EndDate.HasValue)
            coupon.EndDate = new DateTimeOffset(dto.EndDate.Value).ToUnixTimeSeconds();
            
        return coupon;
    }

    private CouponModel MapToCouponModel(UpdateCouponDto dto)
    {
        var coupon = new CouponModel
        {
            Id = dto.Id,
            ProductName = dto.ProductName ?? string.Empty,
            Category = dto.Category ?? string.Empty,
            Description = dto.Description,
            Code = dto.Code ?? string.Empty,
            Amount = dto.Amount,
            Percentage = dto.Percentage ?? 0,
            MinimumAmount = dto.MinimumAmount ?? 0,
            Status = Enum.Parse<Discount.Grpc.CouponStatus>(dto.Status, true),
            IsStackable = dto.IsStackable,
            MaxStackablePercentage = dto.MaxStackablePercentage ?? 0,
            RemainingUses = dto.RemainingUses,
            IsAutomatic = dto.IsAutomatic
        };
        
        coupon.DiscountType = Enum.Parse<Discount.Grpc.DiscountType>(dto.DiscountType, true);
        
        if (dto.StartDate.HasValue)
            coupon.StartDate = new DateTimeOffset(dto.StartDate.Value).ToUnixTimeSeconds();
        if (dto.EndDate.HasValue)
            coupon.EndDate = new DateTimeOffset(dto.EndDate.Value).ToUnixTimeSeconds();
            
        return coupon;
    }

    private CouponDto MapToCouponDto(CouponModel model)
    {
        return new CouponDto
        {
            Id = model.Id,
            ProductName = string.IsNullOrEmpty(model.ProductName) ? null : model.ProductName,
            Category = string.IsNullOrEmpty(model.Category) ? null : model.Category,
            Description = model.Description,
            Code = string.IsNullOrEmpty(model.Code) ? null : model.Code,
            DiscountType = model.DiscountType.ToString(),
            Amount = model.Amount,
            Percentage = model.Percentage == 0 ? null : model.Percentage,
            MinimumAmount = model.MinimumAmount == 0 ? null : model.MinimumAmount,
            StartDate = model.StartDate == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(model.StartDate).DateTime,
            EndDate = model.EndDate == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(model.EndDate).DateTime,
            Status = model.Status.ToString(),
            IsStackable = model.IsStackable,
            MaxStackablePercentage = model.MaxStackablePercentage == 0 ? null : model.MaxStackablePercentage,
            RemainingUses = model.RemainingUses,
            IsAutomatic = model.IsAutomatic
        };
    }
}

