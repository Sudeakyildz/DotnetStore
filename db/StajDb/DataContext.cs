using Microsoft.EntityFrameworkCore;
using StajDb.Models;

namespace StajDb;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<ProductFeatureValue> ProductFeatures => Set<ProductFeatureValue>();
    public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Slug).HasMaxLength(200);

            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.IsDeleted).IsRequired();

            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasMany(e => e.Products)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Description).HasMaxLength(4000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Stock).IsRequired();
            entity.Property(e => e.IsDeleted).IsRequired();

            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => new { e.CategoryId });
        });

        modelBuilder.Entity<Feature>(entity =>
        {
            entity.ToTable("Features");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DataType).HasMaxLength(50);
            entity.Property(e => e.IsDeleted).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<ProductFeatureValue>(entity =>
        {
            entity.ToTable("ProductFeatures");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(1000);

            entity.HasIndex(e => new { e.ProductId, e.FeatureId }).IsUnique();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Product)
                .WithMany(p => p.FeatureValues)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Feature)
                .WithMany(f => f.Values)
                .HasForeignKey(e => e.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductPrice>(entity =>
        {
            entity.ToTable("ProductPrices");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.StartDate).IsRequired();

            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Staj kuralı: her ürün için tek aktif fiyat
            // (EndDate = NULL olan kayıt benzersiz olmalı)
            entity.HasIndex(e => e.ProductId)
                .IsUnique()
                .HasFilter("[EndDate] IS NULL");

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Prices)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        Seed(modelBuilder);
    }

    private static void Seed(ModelBuilder modelBuilder)
    {
        // Basit seed veriler (demo)
        var createdAt = new DateTime(2026, 3, 30, 12, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Telefon", Slug = "telefon", Description = null, ImageUrl = null, IsDeleted = false, CreatedBy = "seed", CreatedAt = createdAt, UpdatedBy = "seed", UpdatedAt = createdAt },
            new Category { Id = 2, Name = "Elektronik", Slug = "elektronik", Description = null, ImageUrl = null, IsDeleted = false, CreatedBy = "seed", CreatedAt = createdAt, UpdatedBy = "seed", UpdatedAt = createdAt }
        );

        modelBuilder.Entity<Feature>().HasData(
            new Feature { Id = 1, Name = "Renk", DataType = "string", IsDeleted = false, CreatedBy = "seed", CreatedAt = createdAt, UpdatedBy = "seed", UpdatedAt = createdAt },
            new Feature { Id = 2, Name = "Depolama", DataType = "string", IsDeleted = false, CreatedBy = "seed", CreatedAt = createdAt, UpdatedBy = "seed", UpdatedAt = createdAt }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                CategoryId = 1,
                Name = "Apple Watch 8",
                Description = "Örnek ürün.",
                Stock = 25,
                Status = "active",
                ImageUrl = "watch-8.jpeg",
                IsDeleted = false,
                CreatedBy = "seed",
                CreatedAt = createdAt,
                UpdatedBy = "seed",
                UpdatedAt = createdAt
            },
            new Product
            {
                Id = 2,
                CategoryId = 1,
                Name = "Apple Watch 9",
                Description = "Örnek ürün.",
                Stock = 10,
                Status = "active",
                ImageUrl = "watch-9.jpeg",
                IsDeleted = false,
                CreatedBy = "seed",
                CreatedAt = createdAt,
                UpdatedBy = "seed",
                UpdatedAt = createdAt
            }
        );

        modelBuilder.Entity<ProductPrice>().HasData(
            new ProductPrice
            {
                Id = 1,
                ProductId = 1,
                Price = 20000m,
                IsDiscount = false,
                StartDate = createdAt,
                EndDate = null,
                CreatedBy = "seed",
                CreatedAt = createdAt,
                UpdatedBy = "seed",
                UpdatedAt = createdAt
            },
            new ProductPrice
            {
                Id = 2,
                ProductId = 2,
                Price = 30000m,
                IsDiscount = false,
                StartDate = createdAt,
                EndDate = null,
                CreatedBy = "seed",
                CreatedAt = createdAt,
                UpdatedBy = "seed",
                UpdatedAt = createdAt
            }
        );

        modelBuilder.Entity<ProductFeatureValue>().HasData(
            new ProductFeatureValue { Id = 1, ProductId = 1, FeatureId = 1, Value = "Siyah", CreatedBy = "seed", CreatedAt = createdAt, UpdatedBy = "seed", UpdatedAt = createdAt },
            new ProductFeatureValue { Id = 2, ProductId = 1, FeatureId = 2, Value = "41mm", CreatedBy = "seed", CreatedAt = createdAt, UpdatedBy = "seed", UpdatedAt = createdAt },
            new ProductFeatureValue { Id = 3, ProductId = 2, FeatureId = 1, Value = "Gümüş", CreatedBy = "seed", CreatedAt = createdAt, UpdatedBy = "seed", UpdatedAt = createdAt },
            new ProductFeatureValue { Id = 4, ProductId = 2, FeatureId = 2, Value = "45mm", CreatedBy = "seed", CreatedAt = createdAt, UpdatedBy = "seed", UpdatedAt = createdAt }
        );
    }
}

