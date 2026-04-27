using System.Globalization;
using DotnetStore.Api.DTOs.Products;
using DotnetStore.Api.Infrastructure;
using DotnetStore.Api.Services.Results;
using Microsoft.EntityFrameworkCore;
using StajDb;
using StajDb.Models;

namespace DotnetStore.Api.Services;

public sealed class ProductService : IProductService
{
    private readonly DataContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _audit;

    public ProductService(DataContext db, ICurrentUser currentUser, IAuditService audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<IReadOnlyList<ProductListItemDto>> SearchAsync(
        string? q,
        int? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        CancellationToken ct)
    {
        IQueryable<Product> queryable = _db.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (categoryId.HasValue)
            queryable = queryable.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            queryable = queryable.Where(p =>
                p.Name.Contains(term) ||
                (p.Description != null && p.Description.Contains(term)));
        }

        if (minPrice.HasValue)
            queryable = queryable.Where(p =>
                p.Prices.Any(pp => pp.EndDate == null && pp.Price >= minPrice.Value));

        if (maxPrice.HasValue)
            queryable = queryable.Where(p =>
                p.Prices.Any(pp => pp.EndDate == null && pp.Price <= maxPrice.Value));

        return await queryable
            .OrderBy(p => p.Name)
            .Select(p => new ProductListItemDto(
                p.Id,
                p.CategoryId,
                p.Category != null ? p.Category.Name : null,
                p.Name,
                p.Stock,
                (int)p.Status,
                p.Prices.Where(pp => pp.EndDate == null).Select(pp => (decimal?)pp.Price).FirstOrDefault(),
                p.ImageUrl))
            .ToListAsync(ct);
    }

    public async Task<ProductDetailDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new ProductDetailDto(
                x.Id,
                x.CategoryId,
                x.Category != null ? x.Category.Name : null,
                x.Name,
                x.Description,
                x.Stock,
                (int)x.Status,
                x.ImageUrl,
                x.Prices.Where(pp => pp.EndDate == null).Select(pp => (decimal?)pp.Price).FirstOrDefault(),
                x.FeatureValues
                    .Select(fv => new ProductFeatureValueDto(
                        fv.FeatureId,
                        fv.Feature != null ? fv.Feature.Name : "",
                        fv.Value))
                    .ToList(),
                x.CreatedByUserId,
                x.UpdatedByUserId,
                x.CreatedAt,
                x.UpdatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AppResult<ProductDetailDto>> CreateAsync(ProductCreateRequest dto, CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(ProductStatus), dto.Status))
            return AppResult<ProductDetailDto>.Fail("Geçersiz Status.", 400);

        var categoryOk = await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId && !c.IsDeleted, ct);
        if (!categoryOk)
            return AppResult<ProductDetailDto>.Fail("Geçersiz ürün grubu (CategoryId).", 400);

        if (dto.FeatureValues is { Count: > 0 })
        {
            var featureIds = dto.FeatureValues.Select(f => f.FeatureId).Distinct().ToList();
            var validCount = await _db.Features.CountAsync(
                f => featureIds.Contains(f.Id) && !f.IsDeleted, ct);
            if (validCount != featureIds.Count)
                return AppResult<ProductDetailDto>.Fail("Geçersiz FeatureId var.", 400);
        }

        var now = DateTime.UtcNow;
        var uid = _currentUser.UserId;

        var product = new Product
        {
            CategoryId = dto.CategoryId,
            Name = dto.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            Stock = dto.Stock,
            Status = (ProductStatus)dto.Status,
            ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim(),
            IsDeleted = false,
            CreatedByUserId = uid,
            CreatedAt = now,
            UpdatedByUserId = uid,
            UpdatedAt = now,
        };

        product.Prices.Add(new ProductPrice
        {
            Price = decimal.Round(dto.Price, 2, MidpointRounding.AwayFromZero),
            IsDiscount = false,
            StartDate = now,
            EndDate = null,
            CreatedByUserId = uid,
            CreatedAt = now,
            UpdatedByUserId = uid,
            UpdatedAt = now,
        });

        if (dto.FeatureValues is { Count: > 0 })
        {
            foreach (var fv in dto.FeatureValues)
            {
                product.FeatureValues.Add(new ProductFeatureValue
                {
                    FeatureId = fv.FeatureId,
                    Value = fv.Value.Trim(),
                    CreatedByUserId = uid,
                    CreatedAt = now,
                    UpdatedByUserId = uid,
                    UpdatedAt = now,
                });
            }
        }

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        if (uid is int u1)
            await _audit.LogAsync(u1, AuditActions.ProductCreate, $"Ürün #{product.Id}: {product.Name}", ct);

        var detail = await GetByIdAsync(product.Id, ct);
        return detail is null
            ? AppResult<ProductDetailDto>.Fail("Ürün oluşturulamadı.", 500)
            : AppResult<ProductDetailDto>.Ok(detail);
    }

    public async Task<AppResult<Unit>> UpdateAsync(int id, ProductUpdateRequest dto, CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(ProductStatus), dto.Status))
            return AppResult<Unit>.Fail("Geçersiz Status.", 400);

        var product = await _db.Products
            .Include(p => p.Prices)
            .Include(p => p.FeatureValues)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (product is null)
            return AppResult<Unit>.Fail("Bulunamadı.", 404);

        var categoryOk = await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId && !c.IsDeleted, ct);
        if (!categoryOk)
            return AppResult<Unit>.Fail("Geçersiz ürün grubu (CategoryId).", 400);

        if (dto.FeatureValues is { Count: > 0 })
        {
            var featureIds = dto.FeatureValues.Select(f => f.FeatureId).Distinct().ToList();
            var validCount = await _db.Features.CountAsync(
                f => featureIds.Contains(f.Id) && !f.IsDeleted, ct);
            if (validCount != featureIds.Count)
                return AppResult<Unit>.Fail("Geçersiz FeatureId var.", 400);
        }

        var now = DateTime.UtcNow;
        var uid = _currentUser.UserId;

        product.CategoryId = dto.CategoryId;
        product.Name = dto.Name.Trim();
        product.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        product.Stock = dto.Stock;
        product.Status = (ProductStatus)dto.Status;
        product.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim();
        product.UpdatedByUserId = uid;
        product.UpdatedAt = now;

        if (dto.NewPrice.HasValue)
        {
            var active = product.Prices.Where(pp => pp.EndDate == null).ToList();
            foreach (var row in active)
            {
                row.EndDate = now;
                row.UpdatedByUserId = uid;
                row.UpdatedAt = now;
            }

            product.Prices.Add(new ProductPrice
            {
                Price = decimal.Round(dto.NewPrice.Value, 2, MidpointRounding.AwayFromZero),
                IsDiscount = false,
                StartDate = now,
                EndDate = null,
                CreatedByUserId = uid,
                CreatedAt = now,
                UpdatedByUserId = uid,
                UpdatedAt = now,
            });
        }

        if (dto.FeatureValues is not null)
        {
            _db.ProductFeatures.RemoveRange(product.FeatureValues);
            product.FeatureValues.Clear();

            foreach (var fv in dto.FeatureValues)
            {
                var row = new ProductFeatureValue
                {
                    ProductId = id,
                    FeatureId = fv.FeatureId,
                    Value = fv.Value.Trim(),
                    CreatedByUserId = uid,
                    CreatedAt = now,
                    UpdatedByUserId = uid,
                    UpdatedAt = now,
                };
                product.FeatureValues.Add(row);
            }
        }

        await _db.SaveChangesAsync(ct);

        if (uid is int u2)
            await _audit.LogAsync(u2, AuditActions.ProductUpdate, $"Ürün #{id}: {product.Name}", ct);

        return AppResult<Unit>.Ok(Unit.Value);
    }

    public async Task<AppResult<Unit>> UpdatePriceOnlyAsync(int id, decimal newPrice, CancellationToken ct)
    {
        var product = await _db.Products
            .Include(p => p.Prices)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (product is null)
            return AppResult<Unit>.Fail("Bulunamadı.", 404);

        var now = DateTime.UtcNow;
        var uid = _currentUser.UserId;
        if (uid is null)
            return AppResult<Unit>.Fail("Oturum yok.", 401);

        var active = product.Prices.Where(pp => pp.EndDate == null).ToList();
        foreach (var row in active)
        {
            row.EndDate = now;
            row.UpdatedByUserId = uid;
            row.UpdatedAt = now;
        }

        product.Prices.Add(new ProductPrice
        {
            Price = decimal.Round(newPrice, 2, MidpointRounding.AwayFromZero),
            IsDiscount = false,
            StartDate = now,
            EndDate = null,
            CreatedByUserId = uid,
            CreatedAt = now,
            UpdatedByUserId = uid,
            UpdatedAt = now,
        });
        product.UpdatedByUserId = uid;
        product.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);

        if (uid is int u3)
            await _audit.LogAsync(
                u3,
                AuditActions.ProductPriceUpdate,
                $"Ürün #{id} {product.Name}: yeni fiyat {newPrice.ToString(CultureInfo.InvariantCulture)} ₺",
                ct);

        return AppResult<Unit>.Ok(Unit.Value);
    }

    public async Task<AppResult<Unit>> UpdateFeatureValuesOnlyAsync(
        int id,
        IReadOnlyList<ProductFeatureValueItem> featureValues,
        CancellationToken ct)
    {
        var product = await _db.Products
            .Include(p => p.FeatureValues)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (product is null)
            return AppResult<Unit>.Fail("Bulunamadı.", 404);

        if (featureValues.Count > 0)
        {
            var featureIds = featureValues.Select(f => f.FeatureId).Distinct().ToList();
            var validCount = await _db.Features.CountAsync(
                f => featureIds.Contains(f.Id) && !f.IsDeleted, ct);
            if (validCount != featureIds.Count)
                return AppResult<Unit>.Fail("Geçersiz FeatureId var.", 400);
        }

        var now = DateTime.UtcNow;
        var uid = _currentUser.UserId;
        if (uid is null)
            return AppResult<Unit>.Fail("Oturum yok.", 401);

        _db.ProductFeatures.RemoveRange(product.FeatureValues);
        product.FeatureValues.Clear();

        foreach (var fv in featureValues)
        {
            product.FeatureValues.Add(new ProductFeatureValue
            {
                FeatureId = fv.FeatureId,
                Value = fv.Value.Trim(),
                CreatedByUserId = uid,
                CreatedAt = now,
                UpdatedByUserId = uid,
                UpdatedAt = now,
            });
        }

        product.UpdatedByUserId = uid;
        product.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);

        if (uid is int u4)
            await _audit.LogAsync(u4, AuditActions.ProductFeaturesUpdate, $"Ürün #{id}: özellik değerleri güncellendi", ct);

        return AppResult<Unit>.Ok(Unit.Value);
    }

    public async Task<AppResult<Unit>> DeleteAsync(int id, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (product is null)
            return AppResult<Unit>.Fail("Bulunamadı.", 404);

        var now = DateTime.UtcNow;
        var name = product.Name;
        product.IsDeleted = true;
        product.UpdatedByUserId = _currentUser.UserId;
        product.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);

        if (_currentUser.UserId is int u5)
            await _audit.LogAsync(u5, AuditActions.ProductDelete, $"Ürün #{id}: {name}", ct);

        return AppResult<Unit>.Ok(Unit.Value);
    }
}
