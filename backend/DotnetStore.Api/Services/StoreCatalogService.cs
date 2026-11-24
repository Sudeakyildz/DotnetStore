using DotnetStore.Api.DTOs.Store;
using Microsoft.EntityFrameworkCore;
using StajDb;
using StajDb.Models;

namespace DotnetStore.Api.Services;

public interface IStoreCatalogService
{
    Task<PagedResult<StoreProductListItem>> SearchAsync(
        string? q, int? categoryId, decimal? minPrice, decimal? maxPrice, bool inStockOnly,
        string? sort, int page, int pageSize, CancellationToken ct);

    Task<StoreProductDetail?> GetAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<StoreCategoryDto>> CategoriesAsync(CancellationToken ct);
    Task<IReadOnlyList<StoreProductListItem>> ByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct);
}

/// <summary>Herkese açık vitrin kataloğu: yalnızca aktif, silinmemiş ve fiyatı olan ürünler.</summary>
public sealed class StoreCatalogService : IStoreCatalogService
{
    private readonly DataContext _db;

    public StoreCatalogService(DataContext db) => _db = db;

    private IQueryable<Product> Visible() =>
        _db.Products.AsNoTracking().Where(p =>
            !p.IsDeleted
            && p.Status == ProductStatus.Active
            && p.Prices.Any(pp => pp.EndDate == null && pp.Price > 0));

    public async Task<PagedResult<StoreProductListItem>> SearchAsync(
        string? q, int? categoryId, decimal? minPrice, decimal? maxPrice, bool inStockOnly,
        string? sort, int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 60);

        var query = Visible();
        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(p => p.Name.Contains(term) || (p.Description != null && p.Description.Contains(term)));
        }

        if (minPrice.HasValue)
            query = query.Where(p => p.Prices.Any(pp => pp.EndDate == null && pp.Price >= minPrice.Value));
        if (maxPrice.HasValue)
            query = query.Where(p => p.Prices.Any(pp => pp.EndDate == null && pp.Price <= maxPrice.Value));
        if (inStockOnly)
            query = query.Where(p => p.Stock > 0);

        var projected = query.Select(p => new
        {
            p.Id,
            p.Name,
            p.CategoryId,
            CategoryName = p.Category != null ? p.Category.Name : null,
            Price = p.Prices.Where(pp => pp.EndDate == null).Select(pp => pp.Price).FirstOrDefault(),
            p.ImageUrl,
            p.Stock,
            p.CreatedAt,
        });

        var total = await projected.CountAsync(ct);
        projected = sort switch
        {
            "priceAsc" => projected.OrderBy(x => x.Price).ThenBy(x => x.Id),
            "priceDesc" => projected.OrderByDescending(x => x.Price).ThenBy(x => x.Id),
            "name" => projected.OrderBy(x => x.Name).ThenBy(x => x.Id),
            _ => projected.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id),
        };

        var rows = await projected.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var items = rows
            .Select(x => new StoreProductListItem(x.Id, x.Name, x.CategoryId, x.CategoryName, x.Price, x.ImageUrl, x.Stock))
            .ToList();
        return new PagedResult<StoreProductListItem>(items, total, page, pageSize);
    }

    public async Task<StoreProductDetail?> GetAsync(int id, CancellationToken ct)
    {
        var p = await Visible()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Description,
                x.CategoryId,
                CategoryName = x.Category != null ? x.Category.Name : null,
                Price = x.Prices.Where(pp => pp.EndDate == null).Select(pp => pp.Price).FirstOrDefault(),
                x.ImageUrl,
                x.Stock,
                Features = x.FeatureValues
                    .Select(fv => new StoreFeatureValue(fv.Feature != null ? fv.Feature.Name : "", fv.Value))
                    .ToList(),
            })
            .FirstOrDefaultAsync(ct);
        if (p is null)
            return null;

        var related = await SearchAsync(null, p.CategoryId, null, null, false, null, 1, 5, ct);
        var relatedItems = related.Items.Where(r => r.Id != p.Id).Take(4).ToList();

        return new StoreProductDetail(
            p.Id, p.Name, p.Description, p.CategoryId, p.CategoryName, p.Price, p.ImageUrl, p.Stock,
            p.Features, relatedItems);
    }

    public async Task<IReadOnlyList<StoreCategoryDto>> CategoriesAsync(CancellationToken ct)
    {
        return await _db.Categories
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .Select(c => new StoreCategoryDto(
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.ImageUrl,
                c.Products.Count(p => !p.IsDeleted
                                      && p.Status == ProductStatus.Active
                                      && p.Prices.Any(pp => pp.EndDate == null && pp.Price > 0))))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StoreProductListItem>> ByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct)
    {
        if (ids.Count == 0)
            return Array.Empty<StoreProductListItem>();

        var rows = await Visible()
            .Where(p => ids.Contains(p.Id))
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                Price = p.Prices.Where(pp => pp.EndDate == null).Select(pp => pp.Price).FirstOrDefault(),
                p.ImageUrl,
                p.Stock,
            })
            .ToListAsync(ct);
        return rows
            .Select(x => new StoreProductListItem(x.Id, x.Name, x.CategoryId, x.CategoryName, x.Price, x.ImageUrl, x.Stock))
            .ToList();
    }
}
