using Discount.Grpc.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Discount.Grpc.Data;

public sealed class DiscountContext(DbContextOptions<DiscountContext> options) : DbContext(options)
{
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<Code> Codes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuration de l'entité Coupon
        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.ToTable("Coupon");
            
            // Sérialiser les catégories en JSON pour SQLite
            entity.Property(e => e.ApplicableCategories)
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => v == null ? null : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>(),
                    new ValueComparer<List<string>>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c != null ? c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())) : 0,
                        c => c == null ? new List<string>() : c.ToList()))
                .HasColumnType("TEXT");
            
            entity.HasData([
                new Coupon 
                {
                    Id = 1, 
                    ProductName = "IPhone X", 
                    Description = "IPhone X New", 
                    Percentage = 30.0,
                    Status = "Active"
                },
                new Coupon 
                {
                    Id = 2, 
                    ProductName = "Samsung 10", 
                    Description = "Samsung 10 New", 
                    Percentage = 50.0,
                    Status = "Active"
                }   
            ]);
        });
        
        // Configuration de l'entité Code
        modelBuilder.Entity<Code>(entity =>
        {
            entity.ToTable("Codes");
            
            // Sérialiser les catégories en JSON pour SQLite
            entity.Property(e => e.ApplicableCategories)
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => v == null ? null : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>(),
                    new ValueComparer<List<string>>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c != null ? c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())) : 0,
                        c => c == null ? new List<string>() : c.ToList()))
                .HasColumnType("TEXT");
            
            // Index sur CodeValue pour recherche rapide
            entity.HasIndex(e => e.CodeValue).IsUnique();
        });
    }
}