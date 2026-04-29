using Microsoft.EntityFrameworkCore;
using ProductManagement.Repository.Models.Domain;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ProductManagement.Repository.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.Property(e => e.ProductId)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.Price)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.Category)
                .HasMaxLength(50);

            entity.Property(e => e.StockQuantity)
                .HasDefaultValue(0);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETDATE()");
        });
    }
}