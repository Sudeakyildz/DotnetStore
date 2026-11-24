using DotnetStore.Api.DTOs.Orders;
using DotnetStore.Api.DTOs.Store;
using DotnetStore.Api.Options;
using DotnetStore.Api.Payments;
using DotnetStore.Api.Services.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StajDb;
using StajDb.Models;

namespace DotnetStore.Api.Services;

public interface ICheckoutService
{
    Task<AppResult<OrderDetailDto>> CheckoutAsync(int userId, CheckoutRequest request, CancellationToken ct);
    Task<AppResult<Unit>> CancelOwnOrderAsync(int userId, int orderId, CancellationToken ct);
}

public sealed class CheckoutService : ICheckoutService
{
    private readonly DataContext _db;
    private readonly IPaymentProvider _payments;
    private readonly IOrderService _orders;
    private readonly IAuditService _audit;
    private readonly StoreOptions _store;
    private readonly ILogger<CheckoutService> _logger;

    public CheckoutService(
        DataContext db,
        IPaymentProvider payments,
        IOrderService orders,
        IAuditService audit,
        IOptions<StoreOptions> store,
        ILogger<CheckoutService> logger)
    {
        _db = db;
        _payments = payments;
        _orders = orders;
        _audit = audit;
        _store = store.Value;
        _logger = logger;
    }

    public async Task<AppResult<OrderDetailDto>> CheckoutAsync(int userId, CheckoutRequest request, CancellationToken ct)
    {
        var cart = await _db.CartItems
            .Where(c => c.UserId == userId)
            .Include(c => c.Product)
            .ThenInclude(p => p!.Prices)
            .ToListAsync(ct);
        if (cart.Count == 0)
            return AppResult<OrderDetailDto>.Fail("Sepetiniz boş.", 400);

        var lines = new List<(int ProductId, string Name, int Qty, decimal Unit)>();
        foreach (var c in cart)
        {
            var p = c.Product!;
            var unit = p.Prices.Where(pp => pp.EndDate == null).Select(pp => (decimal?)pp.Price).FirstOrDefault() ?? 0m;
            if (p.IsDeleted || p.Status != ProductStatus.Active || unit <= 0m)
                return AppResult<OrderDetailDto>.Fail($"\"{p.Name}\" artık satışta değil; lütfen sepetinizden çıkarın.", 400);
            if (c.Quantity > p.Stock)
                return AppResult<OrderDetailDto>.Fail($"\"{p.Name}\" için yeterli stok yok (kalan: {p.Stock}).", 400);
            lines.Add((p.Id, p.Name, c.Quantity, unit));
        }

        var subtotal = lines.Sum(l => l.Qty * l.Unit);
        var shipping = _store.CalculateShipping(subtotal);
        var total = subtotal + shipping;
        var now = DateTime.UtcNow;

        var order = new Order
        {
            CustomerUserId = userId,
            Status = OrderStatus.SiparisAlindi,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            ShippingFullName = request.ShippingFullName.Trim(),
            ShippingPhone = request.ShippingPhone.Trim(),
            ShippingCity = request.ShippingCity.Trim(),
            ShippingAddress = request.ShippingAddress.Trim(),
            ShippingFee = shipping,
            PaymentStatus = PaymentStatus.Pending,
            CreatedByUserId = userId,
            UpdatedByUserId = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };
        foreach (var l in lines)
            order.Items.Add(new OrderItem { ProductId = l.ProductId, Quantity = l.Qty, UnitPrice = l.Unit });

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        // 1) Stoğu atomik olarak ayır (yarış koşulu yok).
        foreach (var l in lines)
        {
            if (!await StockHelper.TryReserveAsync(_db, l.ProductId, l.Qty, ct))
            {
                await tx.RollbackAsync(ct);
                return AppResult<OrderDetailDto>.Fail($"\"{l.Name}\" için stok yetersiz kaldı.", 409);
            }
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        // 2) Ödemeyi çek. Kart bilgisi yalnızca sağlayıcıya iletilir; saklanmaz ve loglanmaz.
        var card = request.Card;
        var charge = await _payments.ChargeAsync(
            new ChargeRequest(total, "TRY", $"ORDER-{order.Id}",
                new CardInfo(card.HolderName, card.Number, card.ExpMonth, card.ExpYear, card.Cvv)),
            ct);

        if (!charge.Success)
        {
            await tx.RollbackAsync(ct); // sipariş ve stok ayırma geri alınır
            await _audit.LogAsync(userId, "payment.failed", $"tutar={total} sebep={charge.Error}", ct);
            return AppResult<OrderDetailDto>.Fail(charge.Error ?? "Ödeme alınamadı.", 402);
        }

        try
        {
            order.PaymentStatus = PaymentStatus.Paid;
            order.Payments.Add(new Payment
            {
                Provider = _payments.Name,
                Amount = total,
                Status = PaymentStatus.Paid,
                TransactionId = charge.TransactionId,
                CardBrand = charge.CardBrand,
                CardLast4 = charge.CardLast4,
                CreatedAt = now,
            });
            await _db.CartItems.Where(c => c.UserId == userId).ExecuteDeleteAsync(ct);
            await _db.SaveChangesAsync(ct);
            await _audit.LogAsync(userId, "order.checkout", $"orderId={order.Id} tutar={total}", ct);
            await tx.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            // Ödeme alındı ama kayıt başarısız: parayı iade etmeyi dene.
            _logger.LogError(ex, "Ödeme alındı fakat sipariş kaydedilemedi; iade deneniyor. tx={Tx}", charge.TransactionId);
            if (charge.TransactionId is not null)
                await _payments.RefundAsync(charge.TransactionId, total, CancellationToken.None);
            return AppResult<OrderDetailDto>.Fail("Siparişiniz kaydedilemedi; ödemeniz iade edildi.", 500);
        }

        var detail = await _orders.GetByIdAsync(order.Id, userId, ct);
        return detail is null
            ? AppResult<OrderDetailDto>.Fail("Sipariş okunamadı.", 500)
            : AppResult<OrderDetailDto>.Ok(detail);
    }

    public async Task<AppResult<Unit>> CancelOwnOrderAsync(int userId, int orderId, CancellationToken ct)
    {
        var order = await _db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerUserId == userId, ct);
        if (order is null)
            return AppResult<Unit>.Fail("Sipariş bulunamadı.", 404);
        if (order.Status != OrderStatus.SiparisAlindi)
            return AppResult<Unit>.Fail("Yalnızca kargoya verilmemiş siparişler iptal edilebilir.", 400);

        return await _orders.TransitionAsync(orderId, OrderStatus.IptalEdildi, userId, ct);
    }
}
