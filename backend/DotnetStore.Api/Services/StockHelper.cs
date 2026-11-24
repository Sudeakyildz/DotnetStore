using Microsoft.EntityFrameworkCore;
using StajDb;

namespace DotnetStore.Api.Services;

/// <summary>Stok değişiklikleri tek SQL UPDATE ile yapılır: eşzamanlı isteklerde stok eksiye düşmez.</summary>
public static class StockHelper
{
    public static async Task<bool> TryReserveAsync(DataContext db, int productId, int qty, CancellationToken ct)
    {
        var rows = await db.Products
            .Where(p => p.Id == productId && !p.IsDeleted && p.Stock >= qty)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - qty), ct);
        return rows == 1;
    }

    public static Task ReleaseAsync(DataContext db, int productId, int qty, CancellationToken ct) =>
        db.Products
            .Where(p => p.Id == productId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + qty), ct);
}
