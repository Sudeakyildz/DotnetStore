using DotnetStore.Api.DTOs.Store;
using DotnetStore.Api.Options;
using DotnetStore.Api.Services.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StajDb;
using StajDb.Models;

namespace DotnetStore.Api.Services;

public interface ICartService
{
    Task<CartDto> GetAsync(int userId, CancellationToken ct);
    Task<AppResult<CartDto>> AddAsync(int userId, int productId, int quantity, CancellationToken ct);
    Task<AppResult<CartDto>> SetQuantityAsync(int userId, int productId, int quantity, CancellationToken ct);
    Task<CartDto> RemoveAsync(int userId, int productId, CancellationToken ct);
    Task ClearAsync(int userId, CancellationToken ct);

    Task<IReadOnlyList<int>> FavoriteIdsAsync(int userId, CancellationToken ct);
    Task<AppResult<Unit>> AddFavoriteAsync(int userId, int productId, CancellationToken ct);
    Task RemoveFavoriteAsync(int userId, int productId, CancellationToken ct);
}

public sealed class CartService : ICartService
{
    private const int MaxPerLine = 20;

    private readonly DataContext _db;
    private readonly StoreOptions _store;

    public CartService(DataContext db, IOptions<StoreOptions> store)
    {
        _db = db;
        _store = store.Value;
    }

    public async Task<CartDto> GetAsync(int userId, CancellationToken ct)
    {
        var rows = await _db.CartItems
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new
            {
                c.ProductId,
                c.Product!.Name,
                c.Product.ImageUrl,
                c.Product.Stock,
                c.Product.IsDeleted,
                c.Product.Status,
                Price = c.Product.Prices.Where(pp => pp.EndDate == null).Select(pp => (decimal?)pp.Price).FirstOrDefault(),
                c.Quantity,
            })
            .ToListAsync(ct);

        var items = rows.Select(r =>
        {
            var unit = r.Price ?? 0m;
            var available = !r.IsDeleted && r.Status == ProductStatus.Active && unit > 0m && r.Stock > 0;
            return new CartItemDto(r.ProductId, r.Name, r.ImageUrl, unit, r.Quantity, r.Stock, unit * r.Quantity, available);
        }).ToList();

        var subtotal = items.Where(i => i.Available).Sum(i => i.LineTotal);
        var shipping = _store.CalculateShipping(subtotal);
        return new CartDto(
            items,
            items.Sum(i => i.Quantity),
            subtotal,
            shipping,
            subtotal + shipping,
            _store.FreeShippingThreshold);
    }

    public async Task<AppResult<CartDto>> AddAsync(int userId, int productId, int quantity, CancellationToken ct)
    {
        if (quantity < 1)
            return AppResult<CartDto>.Fail("Miktar en az 1 olmalı.", 400);

        var product = await _db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted && p.Status == ProductStatus.Active, ct);
        if (product is null)
            return AppResult<CartDto>.Fail("Ürün bulunamadı.", 404);
        if (product.Stock < 1)
            return AppResult<CartDto>.Fail("Bu ürün şu an stokta yok.", 400);

        var cap = Math.Min(MaxPerLine, product.Stock);

        // Aynı ürün için eşzamanlı iki istek gelirse benzersiz indeks çakışır; bir kez daha dene.
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var now = DateTime.UtcNow;
            var line = await _db.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId, ct);
            var newQty = Math.Min((line?.Quantity ?? 0) + quantity, cap);

            if (line is null)
            {
                _db.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = newQty,
                    CreatedAt = now,
                    UpdatedAt = now,
                });
            }
            else
            {
                line.Quantity = newQty;
                line.UpdatedAt = now;
            }

            try
            {
                await _db.SaveChangesAsync(ct);
                break;
            }
            catch (DbUpdateException) when (attempt == 0)
            {
                _db.ChangeTracker.Clear();
            }
        }

        return AppResult<CartDto>.Ok(await GetAsync(userId, ct));
    }

    public async Task<AppResult<CartDto>> SetQuantityAsync(int userId, int productId, int quantity, CancellationToken ct)
    {
        var line = await _db.CartItems
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId, ct);
        if (line is null)
            return AppResult<CartDto>.Fail("Sepette böyle bir ürün yok.", 404);

        if (quantity < 1)
        {
            _db.CartItems.Remove(line);
        }
        else
        {
            line.Quantity = Math.Min(quantity, Math.Max(1, Math.Min(MaxPerLine, line.Product?.Stock ?? 1)));
            line.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return AppResult<CartDto>.Ok(await GetAsync(userId, ct));
    }

    public async Task<CartDto> RemoveAsync(int userId, int productId, CancellationToken ct)
    {
        await _db.CartItems.Where(c => c.UserId == userId && c.ProductId == productId).ExecuteDeleteAsync(ct);
        return await GetAsync(userId, ct);
    }

    public Task ClearAsync(int userId, CancellationToken ct) =>
        _db.CartItems.Where(c => c.UserId == userId).ExecuteDeleteAsync(ct);

    public async Task<IReadOnlyList<int>> FavoriteIdsAsync(int userId, CancellationToken ct) =>
        await _db.Favorites.AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => f.ProductId)
            .ToListAsync(ct);

    public async Task<AppResult<Unit>> AddFavoriteAsync(int userId, int productId, CancellationToken ct)
    {
        var exists = await _db.Products.AnyAsync(p => p.Id == productId && !p.IsDeleted, ct);
        if (!exists)
            return AppResult<Unit>.Fail("Ürün bulunamadı.", 404);
        if (await _db.Favorites.AnyAsync(f => f.UserId == userId && f.ProductId == productId, ct))
            return AppResult<Unit>.Ok(Unit.Value);

        _db.Favorites.Add(new FavoriteProduct { UserId = userId, ProductId = productId, CreatedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync(ct);
        return AppResult<Unit>.Ok(Unit.Value);
    }

    public Task RemoveFavoriteAsync(int userId, int productId, CancellationToken ct) =>
        _db.Favorites.Where(f => f.UserId == userId && f.ProductId == productId).ExecuteDeleteAsync(ct);
}
