using Discount.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Data;

public sealed class DiscountContext(DbContextOptions<DiscountContext> options) : DbContext(options)
{
    public DbSet<Coupon> Coupons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.ToTable("Coupons");
            
            entity.HasKey(c => c.Id);
            
            entity.Property(c => c.ProductName).HasMaxLength(200);
            entity.Property(c => c.Category).HasMaxLength(100);
            entity.Property(c => c.Description).HasMaxLength(500);
            entity.Property(c => c.Code).HasMaxLength(50);
            
            // Index pour améliorer les performances de recherche
            entity.HasIndex(c => c.ProductName);
            entity.HasIndex(c => c.Category);
            entity.HasIndex(c => c.Code);
            entity.HasIndex(c => c.Status);
            entity.HasIndex(c => new { c.Status, c.StartDate, c.EndDate });
            
            // Données initiales (migration)
            entity.HasData([
                new Coupon 
                {
                    Id = 1, 
                    ProductName = "IPhone X", 
                    Description = "IPhone X New", 
                    Amount = 150.0,
                    DiscountType = Models.DiscountType.FixedAmount,
                    Status = Models.CouponStatus.Active,
                    IsAutomatic = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Coupon 
                {
                    Id = 2, 
                    ProductName = "Samsung 10", 
                    Description = "Samsung 10 New", 
                    Amount = 100.0,
                    DiscountType = Models.DiscountType.FixedAmount,
                    Status = Models.CouponStatus.Active,
                    IsAutomatic = true,
                    CreatedAt = DateTime.UtcNow
                }   
            ]);
        });
        
        base.OnModelCreating(modelBuilder);
    }
}