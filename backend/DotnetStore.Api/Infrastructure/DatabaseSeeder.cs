using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StajDb;
using StajDb.Models;

namespace DotnetStore.Api.Infrastructure;

public static class DatabaseSeeder
{
    /// <summary>
    /// Yönetici yoksa oluşturur; ardından örnek kategori, özellik ve ürünleri <strong>eksikse</strong> ekler
    /// (mevcut veritabanında da çalışır — sadece ilk kurulumda değil).
    /// </summary>
    public static async Task SeedAsync(DataContext db, IConfiguration configuration, CancellationToken ct = default)
    {
        var now = new DateTime(2026, 3, 30, 12, 0, 0, DateTimeKind.Utc);
        var hasher = new PasswordHasher<StoreUser>();

        await AlignLegacyAdminAsync(db, configuration, hasher, now, ct);
        var adminId = await EnsureSeedUsersAsync(db, configuration, hasher, now, ct);
        await EnsureMusteriDemoUsersAsync(db, hasher, now, ct);

        var catElektronik = await GetOrCreateCategoryAsync(db, adminId, now, "Elektronik", "elektronik", "Akıllı saat, bilgisayar ve aksesuar.", ct);
        var catGiyim = await GetOrCreateCategoryAsync(db, adminId, now, "Giyim", "giyim", "Kadın, erkek ve çocuk giyim.", ct);

        var legacyTelefon = await db.Categories.FirstOrDefaultAsync(c => c.Slug == "telefon" && !c.IsDeleted, ct);
        if (legacyTelefon is not null)
        {
            await db.Products
                .Where(p => p.CategoryId == legacyTelefon.Id && !p.IsDeleted)
                .ExecuteUpdateAsync(
                    s => s
                        .SetProperty(p => p.CategoryId, catElektronik.Id)
                        .SetProperty(p => p.UpdatedAt, now)
                        .SetProperty(p => p.UpdatedByUserId, adminId),
                    ct);
            legacyTelefon.IsDeleted = true;
            legacyTelefon.UpdatedAt = now;
            legacyTelefon.UpdatedByUserId = adminId;
        }

        var featRenk = await GetOrCreateFeatureAsync(db, adminId, now, "Renk", FeatureDataType.String, ct);
        var featDepolama = await GetOrCreateFeatureAsync(db, adminId, now, "Depolama", FeatureDataType.String, ct);
        var featBeden = await GetOrCreateFeatureAsync(db, adminId, now, "Beden", FeatureDataType.String, ct);

        await RetireNonWwwrootImageProductsAsync(db, adminId, now, ct);

        // Yalnızca wwwroot/images altındaki dosyalara /images/... yolu
        var electronics = new[]
        {
            (
                "Apple iPhone 17 Pro Max 512 GB — Abis",
                "Abis",
                "512 GB",
                99999m,
                8,
                "/images/iphone-17-pro-max-512.jpg",
                "ProMotion teknolojisine sahip 6,9 inç ekran, A19 Pro çip, iPhone’da bugüne kadar en yüksek pil performansı, Pro Fusion kamera, Center Stage ön kamera; abis rengi titanyum kasa (Abis), premium vitrin ürünü."
            ),
            (
                "Apple iPhone 17 Pro Max 512 GB — Gümüş",
                "Gümüş",
                "512 GB",
                99999m,
                6,
                "/images/Iphone17ProMaxGUMUS.jpg",
                "6,9 inç ProMotion, A19 Pro, gelişmiş pil yönetimi, Pro Fusion ve Center Stage. Gümüş tonu titanyum veya paslanmaz detay; günlük ve yoğun kullanıma uygun."
            ),
            (
                "Apple iPhone 17 Pro Max 512 GB — Turuncu (Truncu)",
                "Turuncu",
                "512 GB",
                99999m,
                5,
                "/images/Iphone17ProMaxTruncu.jpg",
                "Aynı 512 GB iPhone 17 Pro Max ailesi; canlı turuncu tonu (görsel dosya adı Truncu), ProMotion, A19 Pro, Pro Fusion ve Center Stage ön kamera."
            ),
        };

        foreach (var (name, color, storage, price, stock, imageUrl, desc) in electronics)
        {
            await EnsureProductMissingAsync(
                db, adminId, now,
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
            (
                "Erkek Basic Tişört",
                "Beyaz",
                "L",
                399m,
                48,
                "/images/ErkekBeyazTshirt.jpg",
                "Erkek cut basic beyaz tişört, günlük kombin. Yumuşak, nefes alan kumaş; sade, düz renk; büyük beden stoku ile uyum."
            ),
            (
                "Kadın Basic Tişört",
                "Beyaz",
                "M",
                389m,
                40,
                "/images/KadınBeyazTshirt.jpg",
                "Kadın formuna uyumlu beyaz basic tişört, şık ve sade. İlkbahar-yaz gardırobuna uyumlu; hafif ve yıkama dayanımı yüksek."
            ),
            (
                "Unisex Siyah Basic Tişört",
                "Siyah",
                "L",
                429m,
                52,
                "/images/unisexSiyahTshirt.jpg",
                "Kadın ve erkeğe uyan unisex siyah model; sade, oversize/rahat kalıp hissi, çok yönlü siyah. Günlük ve okul/iş dışı kombin."
            ),
        };

        foreach (var (name, color, beden, price, stock, imageUrl, desc) in clothing)
        {
            await EnsureProductMissingAsync(
                db, adminId, now,
                name,
                catGiyim, featRenk, featBeden,
                desc,
                stock, price,
                imageUrl,
                (color, beden),
                ct);
        }

        await db.SaveChangesAsync(ct);
        await EnsureMusteriDemoOrdersForAdminPanelAsync(db, adminId, now, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task EnsureMusteriDemoUsersAsync(
        DataContext db,
        PasswordHasher<StoreUser> hasher,
        DateTime now,
        CancellationToken ct)
    {
        var demo = new[]
        {
            ("musteri.ayse@staj.local", "Ayşe T. (Müşteri)", "Musteri!12345"),
            ("musteri.ali@staj.local", "Ali K. (Müşteri)", "Musteri!12345"),
            ("musteri.zehra@staj.local", "Zehra M. (Müşteri)", "Musteri!12345"),
        };
        foreach (var (email, userName, pass) in demo)
        {
            var e = email.ToLowerInvariant();
            if (await db.Users.AnyAsync(u => u.Email == e, ct))
                continue;
            var u = new StoreUser
            {
                UserName = userName,
                Email = e,
                Role = UserRole.Musteri,
                PasswordHash = "",
                CreatedAt = now,
                UpdatedAt = now,
            };
            u.PasswordHash = hasher.HashPassword(u, pass);
            db.Users.Add(u);
        }
    }

    private const string MusteriHareketiTag =
        "Staj demoset 2026: müşteri hareketi (admin panel örnek)";

    /// <summary>
    /// Farklı durumlarda birkaç sipariş; tümü <see cref="UserRole.Musteri"/>. Aynı etiketle kayıt varsa tekrar ekleme.
    /// Gelecekteki müşteri ekranı bu akışa bağlandığında satırlar/hareketler aynı modele eklenebilir.
    /// </summary>
    private static async Task EnsureMusteriDemoOrdersForAdminPanelAsync(
        DataContext db,
        int adminId,
        DateTime now,
        CancellationToken ct)
    {
        // StringComparison’lı Contains EF’de SQL’e çevrilmez; Like kullan.
        if (await db.Orders.AnyAsync(
                o => o.Note != null && EF.Functions.Like(o.Note, "%" + MusteriHareketiTag + "%"),
                cancellationToken: ct))
        {
            return;
        }

        var ayse = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == "musteri.ayse@staj.local", cancellationToken: ct);
        var ali = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == "musteri.ali@staj.local", cancellationToken: ct);
        var zehra = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == "musteri.zehra@staj.local", cancellationToken: ct);
        if (ayse is null || ali is null || zehra is null)
            return;

        var priceByName = await GetOpenUnitPriceByProductNameAsync(db, ct);
        if (priceByName.Count < 2)
            return;

        // (müşteri, durum, not-ek, satırlar, saat-ofset)
        var spec = new (int Cid, OrderStatus Status, string NotePart, (string P, int Q)[] Lines, double HoursAgo)[]
        {
            (ayse.Id, OrderStatus.BegeniyeEklendi, "Favori / beğenildi.", new[] { ("Apple iPhone 17 Pro Max 512 GB — Abis", 1) }, 52),
            (ayse.Id, OrderStatus.Sepette, "Sepet (2 ürün).", new[]
            {
                ("Apple iPhone 17 Pro Max 512 GB — Gümüş", 1),
                ("Erkek Basic Tişört", 2),
            }, 48),
            (ali.Id, OrderStatus.SiparisAlindi, "Ödeme/özet onayı (sipariş alındı).", new[] { ("Unisex Siyah Basic Tişört", 1) }, 36),
            (ali.Id, OrderStatus.KargoyaVerildi, "Kargoya verildi.", new[]
            {
                ("Apple iPhone 17 Pro Max 512 GB — Turuncu (Truncu)", 1),
            }, 24),
            (zehra.Id, OrderStatus.TeslimEdildi, "Teslim edildi.", new[] { ("Kadın Basic Tişört", 2) }, 12),
            (zehra.Id, OrderStatus.IptalEdildi, "Müşteri veya operasyon iptali (örnek).", new[]
            {
                ("Apple iPhone 17 Pro Max 512 GB — Gümüş", 1),
            }, 6),
        };

        foreach (var (cid, st, part, lines, h) in spec)
        {
            var t = now.AddHours(-h);
            var o = new Order
            {
                CustomerUserId = cid,
                Status = st,
                Note = MusteriHareketiTag + " " + part,
                CreatedByUserId = adminId,
                UpdatedByUserId = adminId,
                CreatedAt = t,
                UpdatedAt = t,
            };
            var any = false;
            foreach (var (pName, qty) in lines)
            {
                if (!priceByName.TryGetValue(pName, out var up))
                    continue;
                o.Items.Add(new OrderItem
                {
                    ProductId = up.Id,
                    Quantity = qty,
                    UnitPrice = up.UnitPrice,
                });
                any = true;
            }

            if (any)
                db.Orders.Add(o);
        }
    }

    private sealed class UnitInfo
    {
        public int Id { get; init; }
        public decimal UnitPrice { get; init; }
    }

    private static async Task<Dictionary<string, UnitInfo>> GetOpenUnitPriceByProductNameAsync(
        DataContext db,
        CancellationToken ct)
    {
        var list = await db.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Select(p => new
            {
                p.Name,
                p.Id,
                Open = p.Prices
                    .Where(pr => pr.EndDate == null)
                    .Select(pr => (decimal?)pr.Price)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken: ct);

        var d = new Dictionary<string, UnitInfo>(StringComparer.Ordinal);
        foreach (var x in list)
        {
            if (x.Open is null || x.Open <= 0m)
                continue;
            d[x.Name] = new UnitInfo { Id = x.Id, UnitPrice = x.Open.Value };
        }

        return d;
    }

    /// <summary>
    /// Yalnız <c>wwwroot</c>’tan <c>/images/...</c> yolu dışındaki tohum/dış URL görsellileri kaldırır (yumuşak siler).
    /// </summary>
    private static async Task RetireNonWwwrootImageProductsAsync(
        DataContext db,
        int adminId,
        DateTime now,
        CancellationToken ct)
    {
        // Renk ayrılmadan tek satır kalan iPhone; çift kayıt kalmasın
        const string oldSingleIphone = "Apple iPhone 17 Pro Max 512 GB";
        await db.Products
            .Where(p => !p.IsDeleted && p.Name == oldSingleIphone)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(p => p.IsDeleted, true)
                    .SetProperty(p => p.UpdatedAt, now)
                    .SetProperty(p => p.UpdatedByUserId, adminId),
                cancellationToken: ct);

        // Picsum + http(s) dış linkler. StringComparison’lı Contains SQL’e çevrilmez; Like + tek argümanlı Contains.
        await db.Products
            .Where(
                p => !p.IsDeleted
                     && p.ImageUrl != null
                     && p.ImageUrl != string.Empty
                     && (EF.Functions.Like(p.ImageUrl, "%picsum%")
                         || p.ImageUrl.Contains("://")))
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(p => p.IsDeleted, true)
                    .SetProperty(p => p.UpdatedAt, now)
                    .SetProperty(p => p.UpdatedByUserId, adminId),
                cancellationToken: ct);
    }

    private static async Task AlignLegacyAdminAsync(
        DataContext db,
        IConfiguration configuration,
        PasswordHasher<StoreUser> hasher,
        DateTime now,
        CancellationToken ct)
    {
        var targetEmail = configuration["Seed:AdminEmail"]?.Trim().ToLowerInvariant();
        var targetPassword = configuration["Seed:AdminPassword"];
        if (string.IsNullOrEmpty(targetEmail) || string.IsNullOrEmpty(targetPassword))
            return;

        var legacy = await db.Users.FirstOrDefaultAsync(
            u => u.UserName == "admin" && u.Email.EndsWith("@legacy.local"),
            ct);
        if (legacy is null)
            return;

        legacy.Email = targetEmail;
        legacy.Role = UserRole.Admin;
        legacy.PasswordHash = hasher.HashPassword(legacy, targetPassword);
        legacy.UpdatedAt = now;
        await db.SaveChangesAsync(ct);
    }

    private static async Task<int> EnsureSeedUsersAsync(
        DataContext db,
        IConfiguration configuration,
        PasswordHasher<StoreUser> hasher,
        DateTime now,
        CancellationToken ct)
    {
        var section = configuration.GetSection("Seed:Users");
        if (!section.Exists())
        {
            var fallback = await db.Users.OrderBy(u => u.Id).FirstOrDefaultAsync(ct);
            if (fallback is null)
                throw new InvalidOperationException("Seed:Users tanımlı değil ve veritabanında kullanıcı yok.");
            return fallback.Id;
        }

        foreach (var child in section.GetChildren())
        {
            var email = child["Email"]?.Trim().ToLowerInvariant();
            var userName = child["UserName"]?.Trim();
            var password = child["Password"];
            var roleStr = child["Role"];
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userName) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(roleStr))
                continue;
            if (!Enum.TryParse<UserRole>(roleStr, ignoreCase: true, out var role))
                continue;

            var existing = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
            if (existing is null)
            {
                var u = new StoreUser
                {
                    UserName = userName,
                    Email = email,
                    Role = role,
                    PasswordHash = "",
                    CreatedAt = now,
                    UpdatedAt = now,
                };
                u.PasswordHash = hasher.HashPassword(u, password);
                db.Users.Add(u);
            }
            else
            {
                existing.UserName = userName;
                existing.Role = role;
                existing.PasswordHash = hasher.HashPassword(existing, password);
                existing.UpdatedAt = now;
            }
        }

        await db.SaveChangesAsync(ct);

        var admin = await db.Users.FirstAsync(u => u.Role == UserRole.Admin, ct);
        return admin.Id;
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
