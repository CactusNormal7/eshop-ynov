using Discount.Grpc.Models;
using Mapster;
// Alias pour éviter les conflits entre enums protobuf et modèles C#
using ProtobufDiscountType = Discount.Grpc.DiscountType;
using ProtobufCouponStatus = Discount.Grpc.CouponStatus;

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
        // Pas besoin de Mapster config supplémentaire - on gère manuellement dans les méthodes
    }
    
    /// <summary>
    /// Convertit un CouponModel (protobuf) en Coupon (modèle C#)
    /// </summary>
    public static Coupon ToCoupon(this CouponModel model)
    {
        var coupon = new Coupon
        {
            Id = model.Id,
            ProductName = string.IsNullOrEmpty(model.ProductName) ? null : model.ProductName,
            Category = string.IsNullOrEmpty(model.Category) ? null : model.Category,
            Description = model.Description,
            Code = string.IsNullOrEmpty(model.Code) ? null : model.Code,
            DiscountType = ConvertProtobufDiscountTypeToModel(model.DiscountType),
            Amount = model.Amount,
            Percentage = model.Percentage == 0 ? null : model.Percentage,
            MinimumAmount = model.MinimumAmount == 0 ? null : model.MinimumAmount,
            StartDate = model.StartDate == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(model.StartDate).DateTime,
            EndDate = model.EndDate == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(model.EndDate).DateTime,
            Status = ConvertProtobufCouponStatusToModel(model.Status),
            IsStackable = model.IsStackable,
            MaxStackablePercentage = model.MaxStackablePercentage == 0 ? null : model.MaxStackablePercentage,
            RemainingUses = model.RemainingUses,
            IsAutomatic = model.IsAutomatic
        };
        
        return coupon;
    }
    
    /// <summary>
    /// Convertit un Coupon (modèle C#) en CouponModel (protobuf)
    /// </summary>
    public static CouponModel ToCouponModel(this Coupon coupon)
    {
        var model = new CouponModel
        {
            Id = coupon.Id,
            ProductName = coupon.ProductName ?? string.Empty,
            Category = coupon.Category ?? string.Empty,
            Description = coupon.Description,
            Code = coupon.Code ?? string.Empty,
            DiscountType = ConvertModelDiscountTypeToProtobuf(coupon.DiscountType),
            Amount = coupon.Amount,
            Percentage = coupon.Percentage ?? 0,
            MinimumAmount = coupon.MinimumAmount ?? 0,
            StartDate = coupon.StartDate.HasValue ? new DateTimeOffset(coupon.StartDate.Value).ToUnixTimeSeconds() : 0,
            EndDate = coupon.EndDate.HasValue ? new DateTimeOffset(coupon.EndDate.Value).ToUnixTimeSeconds() : 0,
            Status = ConvertModelCouponStatusToProtobuf(coupon.Status),
            IsStackable = coupon.IsStackable,
            MaxStackablePercentage = coupon.MaxStackablePercentage ?? 0,
            RemainingUses = coupon.RemainingUses,
            IsAutomatic = coupon.IsAutomatic
        };
        return model;
    }
    
    /// <summary>
    /// Convertit DiscountType protobuf vers DiscountType modèle
    /// </summary>
    private static Models.DiscountType ConvertProtobufDiscountTypeToModel(ProtobufDiscountType protobufType)
    {
        return protobufType switch
        {
            ProtobufDiscountType.FixedAmount => Models.DiscountType.FixedAmount,
            ProtobufDiscountType.Percentage => Models.DiscountType.Percentage,
            ProtobufDiscountType.FixedAmountWithCode => Models.DiscountType.FixedAmountWithCode,
            ProtobufDiscountType.Tiered => Models.DiscountType.Tiered,
            _ => Models.DiscountType.FixedAmount
        };
    }
    
    /// <summary>
    /// Convertit DiscountType modèle vers DiscountType protobuf
    /// </summary>
    private static ProtobufDiscountType ConvertModelDiscountTypeToProtobuf(Models.DiscountType modelType)
    {
        return modelType switch
        {
            Models.DiscountType.FixedAmount => ProtobufDiscountType.FixedAmount,
            Models.DiscountType.Percentage => ProtobufDiscountType.Percentage,
            Models.DiscountType.FixedAmountWithCode => ProtobufDiscountType.FixedAmountWithCode,
            Models.DiscountType.Tiered => ProtobufDiscountType.Tiered,
            _ => ProtobufDiscountType.FixedAmount
        };
    }
    
    /// <summary>
    /// Convertit CouponStatus protobuf vers CouponStatus modèle
    /// </summary>
    private static Models.CouponStatus ConvertProtobufCouponStatusToModel(ProtobufCouponStatus protobufStatus)
    {
        return protobufStatus switch
        {
            ProtobufCouponStatus.Active => Models.CouponStatus.Active,
            ProtobufCouponStatus.Expired => Models.CouponStatus.Expired,
            ProtobufCouponStatus.Disabled => Models.CouponStatus.Disabled,
            ProtobufCouponStatus.Upcoming => Models.CouponStatus.Upcoming,
            _ => Models.CouponStatus.Active
        };
    }
    
    /// <summary>
    /// Convertit CouponStatus modèle vers CouponStatus protobuf
    /// </summary>
    private static ProtobufCouponStatus ConvertModelCouponStatusToProtobuf(Models.CouponStatus modelStatus)
    {
        return modelStatus switch
        {
            Models.CouponStatus.Active => ProtobufCouponStatus.Active,
            Models.CouponStatus.Expired => ProtobufCouponStatus.Expired,
            Models.CouponStatus.Disabled => ProtobufCouponStatus.Disabled,
            Models.CouponStatus.Upcoming => ProtobufCouponStatus.Upcoming,
            _ => ProtobufCouponStatus.Active
        };
    }
}
