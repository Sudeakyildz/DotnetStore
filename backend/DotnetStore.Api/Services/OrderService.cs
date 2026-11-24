using DotnetStore.Api.DTOs.Orders;
using DotnetStore.Api.Options;
using DotnetStore.Api.Payments;
using DotnetStore.Api.Services.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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

    public static string ToLabel(PaymentStatus s) => s switch
    {
        PaymentStatus.Pending => "Ödeme bekleniyor",
        PaymentStatus.Paid => "Ödendi",
        PaymentStatus.Failed => "Ödeme başarısız",
        PaymentStatus.Refunded => "İade edildi",
        _ => s.ToString()
    };

    /// <summary>Bu durumdaki siparişler stoktan düşülmüş sayılır.</summary>
    public static bool ConsumesStock(OrderStatus s) =>
        s is OrderStatus.SiparisAlindi or OrderStatus.KargoyaVerildi or OrderStatus.TeslimEdildi;

    public static bool IsAllowedTransition(OrderStatus from, OrderStatus to) => from switch
    {
        OrderStatus.BegeniyeEklendi or OrderStatus.Sepette => true,
        OrderStatus.SiparisAlindi => to is OrderStatus.KargoyaVerildi or OrderStatus.IptalEdildi,
        OrderStatus.KargoyaVerildi => to is OrderStatus.TeslimEdildi or OrderStatus.IptalEdildi,
        _ => false,
    };
}

public sealed class OrderService : IOrderService
{
    private readonly DataContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _audit;
    private readonly IPaymentProvider _payments;
    private readonly StoreOptions _store;

    public OrderService(
        DataContext db,
        ICurrentUser currentUser,
        IAuditService audit,
        IPaymentProvider payments,
        IOptions<StoreOptions> store)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
        _payments = payments;
        _store = store.Value;
    }

    public async Task<IReadOnlyList<OrderListItemDto>> ListAsync(int? customerId, CancellationToken ct)
    {
        var q = _db.Orders.AsNoTracking().AsQueryable();
        if (customerId.HasValue)
            q = q.Where(o => o.CustomerUserId == customerId.Value);

        var rows = await q
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        return rows.Select(o =>
        {
            var names = o.Items.Select(i => i.Product?.Name ?? "Ürün").ToList();
            var preview = names.Count <= 2
                ? string.Join(", ", names)
                : $"{names[0]}, {names[1]} +{names.Count - 2}";
            return new OrderListItemDto(
                o.Id,
                o.CustomerUserId,
                o.Customer?.Email,
                o.Customer?.UserName,
                (int)o.Status,
                OrderStatusHelper.ToLabel(o.Status),
                o.Items.Count,
                o.Items.Sum(i => i.Quantity * i.UnitPrice) + o.ShippingFee,
                o.CreatedAt,
                o.UpdatedAt,
                o.Note,
                (int)o.PaymentStatus,
                OrderStatusHelper.ToLabel(o.PaymentStatus),
                preview,
                o.Items.Select(i => i.Product?.ImageUrl).FirstOrDefault(u => !string.IsNullOrEmpty(u)));
        }).ToList();
    }

    public async Task<OrderSummaryDto> SummaryAsync(CancellationToken ct)
    {
        var pending = await _db.Orders.CountAsync(o => o.Status == OrderStatus.SiparisAlindi, ct);
        var latestId = await _db.Orders.MaxAsync(o => (int?)o.Id, ct) ?? 0;
        var rows = await _db.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.SiparisAlindi)
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new
            {
                o.Id,
                Name = o.Customer != null ? o.Customer.UserName : null,
                Items = o.Items.Sum(i => (decimal?)(i.Quantity * i.UnitPrice)) ?? 0m,
                o.ShippingFee,
                o.CreatedAt,
            })
            .ToListAsync(ct);
        return new OrderSummaryDto(
            pending,
            latestId,
            rows.Select(r => new OrderNotificationDto(r.Id, r.Name, r.Items + r.ShippingFee, r.CreatedAt)).ToList());
    }

    public async Task<OrderDetailDto?> GetByIdAsync(int id, int? customerId, CancellationToken ct)
    {
        var o = await _db.Orders
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Payments)
            .Include(x => x.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (o is null)
            return null;
        if (customerId.HasValue && o.CustomerUserId != customerId.Value)
            return null;

        var itemRows = o.Items
            .Select(i => new OrderItemRowDto(
                i.ProductId,
                i.Product?.Name,
                i.Quantity,
                i.UnitPrice,
                i.Quantity * i.UnitPrice,
                i.Product?.ImageUrl))
            .ToList();
        var total = itemRows.Sum(r => r.LineTotal) + o.ShippingFee;
        var card = o.Payments
            .Where(p => p.Status == PaymentStatus.Paid)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => p.CardLast4 is null ? null : $"{p.CardBrand} •••• {p.CardLast4}")
            .FirstOrDefault();
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
            o.UpdatedAt,
            o.ShippingFullName,
            o.ShippingPhone,
            o.ShippingCity,
            o.ShippingAddress,
            o.ShippingFee,
            (int)o.PaymentStatus,
            OrderStatusHelper.ToLabel(o.PaymentStatus),
            o.TrackingNumber,
            card,
            o.Status == OrderStatus.SiparisAlindi);
    }

    public async Task<AppResult<OrderDetailDto>> CreateAsync(OrderCreateRequest dto, CancellationToken ct)
    {
        var adminId = _currentUser.UserId ?? 0;
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
            return AppResult<OrderDetailDto>.Fail("Sipariş yalnızca müşteri (Musteri) hesabına atanabilir.", 400);

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
        var priceById = openPrices.ToDictionary(p => p.ProductId, p => p.Price);

        var lines = new List<(int ProductId, int Qty, decimal UnitPrice)>();
        foreach (var p in products)
        {
            if (!priceById.TryGetValue(p.Id, out var unit) || unit <= 0m)
                return AppResult<OrderDetailDto>.Fail($"\"{p.Name}\" için geçerli fiyat yok.", 400);
            var q = merged[p.Id];
            if (OrderStatusHelper.ConsumesStock(status) && q > p.Stock)
                return AppResult<OrderDetailDto>.Fail($"'{p.Name}' için stok yetersiz (kalan: {p.Stock}).", 400);
            lines.Add((p.Id, q, unit));
        }

        var subtotal = lines.Sum(l => l.Qty * l.UnitPrice);
        var order = new Order
        {
            CustomerUserId = dto.CustomerUserId,
            Status = status,
            Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim(),
            ShippingFee = OrderStatusHelper.ConsumesStock(status) ? _store.CalculateShipping(subtotal) : 0m,
            PaymentStatus = PaymentStatus.Pending,
            CreatedByUserId = adminId,
            UpdatedByUserId = adminId,
            CreatedAt = now,
            UpdatedAt = now,
        };
        foreach (var (productId, qty, unit) in lines)
            order.Items.Add(new OrderItem { ProductId = productId, Quantity = qty, UnitPrice = unit });

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        if (OrderStatusHelper.ConsumesStock(status))
        {
            foreach (var (productId, qty, _) in lines)
            {
                if (!await StockHelper.TryReserveAsync(_db, productId, qty, ct))
                {
                    await tx.RollbackAsync(ct);
                    return AppResult<OrderDetailDto>.Fail("Stok yetersiz; sipariş oluşturulamadı.", 400);
                }
            }
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(
            adminId,
            "order.create",
            $"orderId={order.Id} müşteri={dto.CustomerUserId} durum={status}",
            ct);
        await tx.CommitAsync(ct);

        var created = await GetByIdAsync(order.Id, null, ct);
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

        return await TransitionAsync(id, (OrderStatus)dto.Status, adminId, ct);
    }

    public async Task<AppResult<Unit>> TransitionAsync(int id, OrderStatus newStatus, int actorId, CancellationToken ct)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
            return AppResult<Unit>.Fail("Sipariş bulunamadı.", 404);

        var before = order.Status;
        if (before == newStatus)
            return AppResult<Unit>.Ok(Unit.Value);

        if (!OrderStatusHelper.IsAllowedTransition(before, newStatus))
        {
            return AppResult<Unit>.Fail(
                $"\"{OrderStatusHelper.ToLabel(before)}\" durumundan \"{OrderStatusHelper.ToLabel(newStatus)}\" durumuna geçilemez.",
                400);
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        var wasConsuming = OrderStatusHelper.ConsumesStock(before);
        var willConsume = OrderStatusHelper.ConsumesStock(newStatus);

        if (wasConsuming && !willConsume)
        {
            foreach (var i in order.Items)
                await StockHelper.ReleaseAsync(_db, i.ProductId, i.Quantity, ct);
        }
        else if (!wasConsuming && willConsume)
        {
            foreach (var i in order.Items)
            {
                if (!await StockHelper.TryReserveAsync(_db, i.ProductId, i.Quantity, ct))
                {
                    await tx.RollbackAsync(ct);
                    return AppResult<Unit>.Fail("Stok yetersiz olduğundan durum değiştirilemedi.", 400);
                }
            }
        }

        var now = DateTime.UtcNow;
        if (newStatus == OrderStatus.IptalEdildi && order.PaymentStatus == PaymentStatus.Paid)
        {
            var paid = order.Payments
                .Where(p => p.Status == PaymentStatus.Paid && p.TransactionId != null)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefault();
            if (paid is not null)
            {
                var refund = await _payments.RefundAsync(paid.TransactionId!, paid.Amount, ct);
                if (!refund.Success)
                {
                    await tx.RollbackAsync(ct);
                    return AppResult<Unit>.Fail("Ödeme iadesi yapılamadı: " + refund.Error, 502);
                }

                order.Payments.Add(new Payment
                {
                    Provider = _payments.Name,
                    Amount = paid.Amount,
                    Status = PaymentStatus.Refunded,
                    TransactionId = refund.TransactionId,
                    CardBrand = paid.CardBrand,
                    CardLast4 = paid.CardLast4,
                    CreatedAt = now,
                });
            }

            order.PaymentStatus = PaymentStatus.Refunded;
        }

        if (newStatus == OrderStatus.KargoyaVerildi && string.IsNullOrEmpty(order.TrackingNumber))
            order.TrackingNumber = "TRK" + Random.Shared.NextInt64(100_000_000_000, 999_999_999_999);

        order.Status = newStatus;
        order.UpdatedByUserId = actorId;
        order.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(actorId, "order.status", $"orderId={id} {before}→{newStatus}", ct);
        await tx.CommitAsync(ct);
        return AppResult<Unit>.Ok(Unit.Value);
    }
}
