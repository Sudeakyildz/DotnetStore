namespace DotnetStore.Api.DTOs.Orders;

public sealed record OrderListItemDto(
    int Id,
    int CustomerUserId,
    string? CustomerEmail,
    string? CustomerName,
    int Status,
    string StatusLabel,
    int ItemCount,
    decimal Total,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? Note,
    int PaymentStatus = 0,
    string PaymentStatusLabel = "",
    string? ItemsPreview = null,
    string? FirstImageUrl = null
);

public sealed record OrderNotificationDto(int Id, string? CustomerName, decimal Total, DateTime CreatedAt);

public sealed record OrderSummaryDto(int PendingCount, int LatestOrderId, IReadOnlyList<OrderNotificationDto> Latest);
