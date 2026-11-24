using DotnetStore.Api.DTOs.Orders;
using DotnetStore.Api.Services.Results;
using StajDb.Models;

namespace DotnetStore.Api.Services;

public interface IOrderService
{
    /// <param name="customerId">Verilirse yalnızca o müşterinin siparişleri.</param>
    Task<IReadOnlyList<OrderListItemDto>> ListAsync(int? customerId, CancellationToken ct);

    /// <param name="customerId">Verilirse sipariş bu müşteriye ait değilse null döner.</param>
    Task<OrderDetailDto?> GetByIdAsync(int id, int? customerId, CancellationToken ct);

    Task<OrderSummaryDto> SummaryAsync(CancellationToken ct);

    Task<AppResult<OrderDetailDto>> CreateAsync(OrderCreateRequest dto, CancellationToken ct);
    Task<AppResult<Unit>> UpdateStatusAsync(int id, OrderStatusUpdateRequest dto, CancellationToken ct);

    /// <summary>Durum geçişi: kuralları uygular, stoğu iade eder/düşer, iptalde ödemeyi iade eder.</summary>
    Task<AppResult<Unit>> TransitionAsync(int id, OrderStatus newStatus, int actorId, CancellationToken ct);
}
