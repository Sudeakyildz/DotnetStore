using DotnetStore.Api.DTOs.Orders;
using DotnetStore.Api.Services.Results;
using Microsoft.EntityFrameworkCore;
using StajDb;
using StajDb.Models;

namespace DotnetStore.Api.Services;

public static class OrderStatusHelper
{
    public static string ToLabel(OrderStatus s) => s switch
    {
        OrderStatus.BegeniyeEklendi => "Beğeniye eklendi",
        OrderStatus.Sepette => "Sepette",
        OrderStatus.SiparisAlindi => "Sipariş alındı",
        OrderStatus.KargoyaVerildi => "Kargoya verildi",
        OrderStatus.TeslimEdildi => "Teslim edildi",
        OrderStatus.IptalEdildi => "İptal edildi",
        _ => s.ToString()
    };
}

public sealed class OrderService : IOrderService
{
    private readonly DataContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _audit;

    public OrderService(DataContext db, ICurrentUser currentUser, IAuditService audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<IReadOnlyList<OrderListItemDto>> ListAsync(CancellationToken ct)
    {
        return await _db.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListItemDto(
                o.Id,
                o.CustomerUserId,
                o.Customer != null ? o.Customer.Email : null,
                o.Customer != null ? o.Customer.UserName : null,
                (int)o.Status,
                OrderStatusHelper.ToLabel(o.Status),
                o.Items.Count,
                o.Items.Sum(i => (decimal?)(i.Quantity * i.UnitPrice)) ?? 0m,
                o.CreatedAt,
                o.UpdatedAt,
                o.Note
            ))
            .ToListAsync(ct);
    }

    public async Task<OrderDetailDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var o = await _db.Orders
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (o is null)
            return null;

        var itemRows = o.Items
            .Select(i => new OrderItemRowDto(
                i.ProductId,
                i.Product?.Name,
                i.Quantity,
                i.UnitPrice,
                i.Quantity * i.UnitPrice))
            .ToList();
        var total = itemRows.Sum(r => r.LineTotal);
        return new OrderDetailDto(
            o.Id,
            o.CustomerUserId,
            o.Customer?.Email,
            o.Customer?.UserName,
            (int)o.Status,
            OrderStatusHelper.ToLabel(o.Status),
            o.Note,
            itemRows,
            total,
            o.CreatedByUserId,
            o.UpdatedByUserId,
            o.CreatedAt,
            o.UpdatedAt);
    }

    public async Task<AppResult<OrderDetailDto>> CreateAsync(OrderCreateRequest dto, CancellationToken ct)
    {
        var adminId = _currentUser.UserId
                        ?? 0;
        if (adminId == 0)
            return AppResult<OrderDetailDto>.Fail("Kullanıcı id bulunamadı.", 401);

        if (dto.Items is null || dto.Items.Count == 0)
            return AppResult<OrderDetailDto>.Fail("En az bir ürün satırı gerekli.", 400);

        var statusRaw = dto.Status ?? (int)OrderStatus.SiparisAlindi;
        if (!Enum.IsDefined(typeof(OrderStatus), statusRaw))
            return AppResult<OrderDetailDto>.Fail("Geçersiz durum (Status).", 400);
        var status = (OrderStatus)statusRaw;

        var customer = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == dto.CustomerUserId, ct);
        if (customer is null)
            return AppResult<OrderDetailDto>.Fail("Müşteri (kullanıcı) bulunamadı.", 404);
        if (customer.Role != UserRole.Musteri)
        {
            return AppResult<OrderDetailDto>.Fail(
                "Sipariş yalnızca mağaza müşterisi (Musteri) hesabına atanabilir; personel ve fiyat yöneticisi dışı.",
                400);
        }

        var merged = dto.Items
            .GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));
        if (merged.Values.Any(q => q < 1))
            return AppResult<OrderDetailDto>.Fail("Miktar en az 1 olmalı.", 400);

        var now = DateTime.UtcNow;
        var productIds = merged.Keys.ToList();
        var products = await _db.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted && productIds.Contains(p.Id))
            .ToListAsync(ct);
        if (products.Count != productIds.Count)
            return AppResult<OrderDetailDto>.Fail("Bir veya daha fazla ürün bulunamadı veya arşivde.", 404);

        var openPrices = await _db.ProductPrices
            .AsNoTracking()
            .Where(pp => productIds.Contains(pp.ProductId) && pp.EndDate == null)
            .ToListAsync(ct);
        if (openPrices.GroupBy(p => p.ProductId).Any(g => g.Count() > 1))
            return AppResult<OrderDetailDto>.Fail("Aynı ürün için birden fazla açık fiyat; veriyi düzeltin.", 500);
        var priceById = openPrices.ToDictionary(p => p.ProductId, p => p.Price);

        var lines = new List<(int ProductId, int Qty, decimal UnitPrice)>();
        foreach (var p in products)
        {
            if (!priceById.TryGetValue(p.Id, out var unit) || unit <= 0m)
                return AppResult<OrderDetailDto>.Fail($"\"{p.Name}\" için geçerli fiyat yok.", 400);
            var q = merged[p.Id];
            if (q > p.Stock)
                return AppResult<OrderDetailDto>.Fail(
                    $"'{p.Name}' için stok yetersiz (kalan: {p.Stock}).",
                    400);
            lines.Add((p.Id, q, unit));
        }

        var order = new Order
        {
            CustomerUserId = dto.CustomerUserId,
            Status = status,
            Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim(),
            CreatedByUserId = adminId,
            UpdatedByUserId = adminId,
            CreatedAt = now,
            UpdatedAt = now,
        };
        foreach (var (productId, qty, unit) in lines)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = productId,
                Quantity = qty,
                UnitPrice = unit,
            });
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(
            adminId,
            "order.create",
            $"orderId={order.Id} müşteri={dto.CustomerUserId} durum={status}",
            ct);

        var created = await GetByIdAsync(order.Id, ct);
        return created is not null
            ? AppResult<OrderDetailDto>.Ok(created)
            : AppResult<OrderDetailDto>.Fail("Sipariş okunamadı.", 500);
    }

    public async Task<AppResult<Unit>> UpdateStatusAsync(int id, OrderStatusUpdateRequest dto, CancellationToken ct)
    {
        var adminId = _currentUser.UserId ?? 0;
        if (adminId == 0)
            return AppResult<Unit>.Fail("Kullanıcı id bulunamadı.", 401);

        if (!Enum.IsDefined(typeof(OrderStatus), dto.Status))
            return AppResult<Unit>.Fail("Geçersiz durum (Status).", 400);

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
            return AppResult<Unit>.Fail("Sipariş bulunamadı.", 404);

        var newStatus = (OrderStatus)dto.Status;
        if (order.Status == newStatus)
        {
            await _audit.LogAsync(
                adminId,
                "order.status",
                $"orderId={id} durum aynı={newStatus} (güncelleme)",
                ct);
            return AppResult<Unit>.Ok(default);
        }

        var before = order.Status;
        var now = DateTime.UtcNow;
        order.Status = newStatus;
        order.UpdatedByUserId = adminId;
        order.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(
            adminId,
            "order.status",
            $"orderId={id} {before}→{newStatus}",
            ct);
        return AppResult<Unit>.Ok(default);
    }
}
