using Discount.Grpc.Models;
using Mapster;

namespace Discount.Grpc.Services;

/// <summary>
/// Extensions pour mapper entre les types protobuf et les modèles C#
/// </summary>
public static class DiscountMappingExtensions
{
    /// <summary>
    /// Configure le mapping Mapster pour les conversions
    /// </summary>
    public static void ConfigureMapping()
    {
        TypeAdapterConfig<CouponModel, Coupon>.NewConfig()
            .Map(dest => dest.DiscountType, src => (DiscountType)src.DiscountType)
            .Map(dest => dest.Status, src => (CouponStatus)src.Status)
            .Map(dest => dest.StartDate, src => src.StartDate == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(src.StartDate).DateTime)
            .Map(dest => dest.EndDate, src => src.EndDate == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(src.EndDate).DateTime);
        
        TypeAdapterConfig<Coupon, CouponModel>.NewConfig()
            .Map(dest => dest.DiscountType, src => (DiscountProtoService.DiscountType)src.DiscountType)
            .Map(dest => dest.Status, src => (DiscountProtoService.CouponStatus)src.Status)
            .Map(dest => dest.StartDate, src => src.StartDate.HasValue ? new DateTimeOffset(src.StartDate.Value).ToUnixTimeSeconds() : 0)
            .Map(dest => dest.EndDate, src => src.EndDate.HasValue ? new DateTimeOffset(src.EndDate.Value).ToUnixTimeSeconds() : 0);
    }
    
    public static Coupon ToCoupon(this CouponModel model)
    {
        var coupon = model.Adapt<Coupon>();
        coupon.DiscountType = (DiscountType)model.DiscountType;
        coupon.Status = (CouponStatus)model.Status;
        
        if (model.StartDate != 0)
            coupon.StartDate = DateTimeOffset.FromUnixTimeSeconds(model.StartDate).DateTime;
        if (model.EndDate != 0)
            coupon.EndDate = DateTimeOffset.FromUnixTimeSeconds(model.EndDate).DateTime;
            
        return coupon;
    }
    
    public static CouponModel ToCouponModel(this Coupon coupon)
    {
        var model = new CouponModel
        {
            Id = coupon.Id,
            ProductName = coupon.ProductName ?? string.Empty,
            Category = coupon.Category ?? string.Empty,
            Description = coupon.Description,
            Code = coupon.Code ?? string.Empty,
            DiscountType = (DiscountProtoService.DiscountType)(int)coupon.DiscountType,
            Amount = coupon.Amount,
            Percentage = coupon.Percentage ?? 0,
            MinimumAmount = coupon.MinimumAmount ?? 0,
            StartDate = coupon.StartDate.HasValue ? new DateTimeOffset(coupon.StartDate.Value).ToUnixTimeSeconds() : 0,
            EndDate = coupon.EndDate.HasValue ? new DateTimeOffset(coupon.EndDate.Value).ToUnixTimeSeconds() : 0,
            Status = (DiscountProtoService.CouponStatus)(int)coupon.Status,
            IsStackable = coupon.IsStackable,
            MaxStackablePercentage = coupon.MaxStackablePercentage ?? 0,
            RemainingUses = coupon.RemainingUses,
            IsAutomatic = coupon.IsAutomatic
        };
        return model;
    }
}

