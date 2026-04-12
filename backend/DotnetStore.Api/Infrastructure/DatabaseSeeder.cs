using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StajDb;
using StajDb.Models;

namespace DotnetStore.Api.Infrastructure;

public static class DatabaseSeeder
{
    /// <summary>
    /// Yönetici yoksa oluşturur; ardından örnek kategori, özellik ve ürünleri <strong>eksikse</strong> ekler
    /// (mevcut veritabanında da çalışır — sadece ilk kurulumda değil).
    /// </summary>
    public static async Task SeedAsync(DataContext db, CancellationToken ct = default)
    {
        var now = new DateTime(2026, 3, 30, 12, 0, 0, DateTimeKind.Utc);
        var hasher = new PasswordHasher<StoreUser>();

        var admin = await db.Users.FirstOrDefaultAsync(u => u.UserName == "admin", ct);
        if (admin is null)
        {
            admin = new StoreUser
            {
                UserName = "admin",
                PasswordHash = "",
                CreatedAt = now,
                UpdatedAt = now,
            };
            admin.PasswordHash = hasher.HashPassword(admin, "admin123");
            db.Users.Add(admin);
            await db.SaveChangesAsync(ct);
        }

        var uid = admin.Id;

        var catElektronik = await GetOrCreateCategoryAsync(db, uid, now, "Elektronik", "elektronik", "Akıllı saat, bilgisayar ve aksesuar.", ct);
        var catGiyim = await GetOrCreateCategoryAsync(db, uid, now, "Giyim", "giyim", "Kadın, erkek ve çocuk giyim.", ct);

        var legacyTelefon = await db.Categories.FirstOrDefaultAsync(c => c.Slug == "telefon" && !c.IsDeleted, ct);
        if (legacyTelefon is not null)
        {
            await db.Products
                .Where(p => p.CategoryId == legacyTelefon.Id && !p.IsDeleted)
                .ExecuteUpdateAsync(
                    s => s
                        .SetProperty(p => p.CategoryId, catElektronik.Id)
                        .SetProperty(p => p.UpdatedAt, now)
                        .SetProperty(p => p.UpdatedByUserId, uid),
                    ct);
            legacyTelefon.IsDeleted = true;
            legacyTelefon.UpdatedAt = now;
            legacyTelefon.UpdatedByUserId = uid;
        }

        var featRenk = await GetOrCreateFeatureAsync(db, uid, now, "Renk", FeatureDataType.String, ct);
        var featDepolama = await GetOrCreateFeatureAsync(db, uid, now, "Depolama", FeatureDataType.String, ct);
        var featBeden = await GetOrCreateFeatureAsync(db, uid, now, "Beden", FeatureDataType.String, ct);

        await EnsureProductMissingAsync(
            db, uid, now,
            "Apple Watch 8",
            catElektronik, featRenk, featDepolama,
            "Örnek ürün.",
            25, 20000m,
            "https://picsum.photos/seed/staj-watch-8/480/600",
            ("Siyah", "41mm"),
            ct);

        await EnsureProductMissingAsync(
            db, uid, now,
            "Apple Watch 9",
            catElektronik, featRenk, featDepolama,
            "Örnek ürün.",
            10, 30000m,
            "https://picsum.photos/seed/staj-watch-9/480/600",
            ("Gümüş", "45mm"),
            ct);

        var electronics = new[]
        {
            ("Lenovo IdeaPad 5 Laptop", "Gri", "512 GB SSD", 18999m, 6, "https://picsum.photos/seed/staj-elec-01/480/600", "15,6 inç FHD, günlük ve ofis kullanımı."),
            ("Logitech MX Master 3S Mouse", "Grafit", "—", 3299m, 22, "https://picsum.photos/seed/staj-elec-02/480/600", "Sessiz tıklama, çoklu cihaz eşlemesi."),
            ("JBL Flip 6 Taşınabilir Hoparlör", "Mavi", "—", 3490m, 15, "https://picsum.photos/seed/staj-elec-03/480/600", "IP67 su geçirmez, 12 saat çalma."),
            ("Samsung 25W USB-C Şarj Adaptörü", "Beyaz", "—", 449m, 40, "https://picsum.photos/seed/staj-elec-04/480/600", "Hızlı şarj, kompakt tasarım."),
            ("TP-Link Archer AX53 Router", "Siyah", "—", 2199m, 11, "https://picsum.photos/seed/staj-elec-05/480/600", "Wi-Fi 6, çift bant, ev ofis."),
            ("Anker PowerCore 20000 mAh", "Siyah", "—", 1199m, 28, "https://picsum.photos/seed/staj-elec-06/480/600", "USB-C ve USB-A çıkışları."),
            ("Dell P2422H 24 inç Monitör", "Siyah", "—", 5499m, 9, "https://picsum.photos/seed/staj-elec-07/480/600", "IPS, Full HD, ayarlanabilir ayak."),
            ("Logitech K380 Çoklu Cihaz Klavye", "Pembe", "—", 899m, 35, "https://picsum.photos/seed/staj-elec-08/480/600", "Kompakt, sessiz tuşlar, Bluetooth."),
        };

        foreach (var (name, color, storage, price, stock, imageUrl, desc) in electronics)
        {
            await EnsureProductMissingAsync(
                db, uid, now,
                name,
                catElektronik, featRenk, featDepolama,
                desc,
                stock, price,
                imageUrl,
                (color, storage),
                ct);
        }

        var clothing = new[]
        {
            ("Kadın Klasik Palto", "Bej", "M", 2499m, 12, "https://picsum.photos/seed/staj-giyim-01/480/600", "Yün karışımlı, cepli."),
            ("Erkek Basic Tişört Paketi (3'lü)", "Beyaz", "L", 399m, 45, "https://picsum.photos/seed/staj-giyim-02/480/600", "%100 pamuk, regular fit."),
            ("Çocuk Polar Mont", "Lacivert", "8 yaş", 599m, 20, "https://picsum.photos/seed/staj-giyim-03/480/600", "Hafif, yıkanabilir."),
        };

        foreach (var (name, color, beden, price, stock, imageUrl, desc) in clothing)
        {
            await EnsureProductMissingAsync(
                db, uid, now,
                name,
                catGiyim, featRenk, featBeden,
                desc,
                stock, price,
                imageUrl,
                (color, beden),
                ct);
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task<Category> GetOrCreateCategoryAsync(
        DataContext db,
        int uid,
        DateTime now,
        string name,
        string slug,
        string? description,
        CancellationToken ct)
    {
        // Slug benzersiz indeksi silinmiş (IsDeleted) satırlarda da geçerlidir; yalnızca !IsDeleted aranırsa çift INSERT hatası oluşur.
        var existing = await db.Categories.FirstOrDefaultAsync(c => c.Slug == slug, ct);
        if (existing is not null)
        {
            if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                existing.Name = name;
                if (description is not null)
                    existing.Description = description;
                existing.UpdatedAt = now;
                existing.UpdatedByUserId = uid;
            }

            return existing;
        }

        var c = new Category
        {
            Name = name,
            Slug = slug,
            Description = description,
            IsDeleted = false,
            CreatedByUserId = uid,
            UpdatedByUserId = uid,
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.Categories.Add(c);
        return c;
    }

    private static async Task<Feature> GetOrCreateFeatureAsync(
        DataContext db,
        int uid,
        DateTime now,
        string name,
        FeatureDataType dataType,
        CancellationToken ct)
    {
        var existing = await db.Features.FirstOrDefaultAsync(f => f.Name == name, ct);
        if (existing is not null)
        {
            if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                existing.DataType = dataType;
                existing.UpdatedAt = now;
                existing.UpdatedByUserId = uid;
            }

            return existing;
        }

        var f = new Feature
        {
            Name = name,
            DataType = dataType,
            IsDeleted = false,
            CreatedByUserId = uid,
            UpdatedByUserId = uid,
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.Features.Add(f);
        return f;
    }

    private static async Task EnsureProductMissingAsync(
        DataContext db,
        int uid,
        DateTime now,
        string name,
        Category category,
        Feature featureFirst,
        Feature featureSecond,
        string description,
        int stock,
        decimal price,
        string imageUrl,
        (string FirstValue, string SecondValue) specs,
        CancellationToken ct)
    {
        var exists = await db.Products.AnyAsync(p => p.Name == name && !p.IsDeleted, ct);
        if (exists)
            return;

        var p = new Product
        {
            Category = category,
            Name = name,
            Description = description,
            Stock = stock,
            Status = ProductStatus.Active,
            ImageUrl = imageUrl,
            IsDeleted = false,
            CreatedByUserId = uid,
            UpdatedByUserId = uid,
            CreatedAt = now,
            UpdatedAt = now,
        };
        p.Prices.Add(new ProductPrice
        {
            Price = price,
            IsDiscount = false,
            StartDate = now,
            EndDate = null,
            CreatedByUserId = uid,
            UpdatedByUserId = uid,
            CreatedAt = now,
            UpdatedAt = now,
        });
        p.FeatureValues.Add(new ProductFeatureValue
        {
            Feature = featureFirst,
            Value = specs.FirstValue,
            CreatedByUserId = uid,
            UpdatedByUserId = uid,
            CreatedAt = now,
            UpdatedAt = now,
        });
        p.FeatureValues.Add(new ProductFeatureValue
        {
            Feature = featureSecond,
            Value = specs.SecondValue,
            CreatedByUserId = uid,
            UpdatedByUserId = uid,
            CreatedAt = now,
            UpdatedAt = now,
        });
        db.Products.Add(p);
    }
}
