using DotnetStore.Api.DTOs.Orders;
using DotnetStore.Api.Services.Results;

namespace DotnetStore.Api.Services;

public interface IOrderService
{
    Task<IReadOnlyList<OrderListItemDto>> ListAsync(CancellationToken ct);
    Task<OrderDetailDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<AppResult<OrderDetailDto>> CreateAsync(OrderCreateRequest dto, CancellationToken ct);
    Task<AppResult<Unit>> UpdateStatusAsync(int id, OrderStatusUpdateRequest dto, CancellationToken ct);
}
