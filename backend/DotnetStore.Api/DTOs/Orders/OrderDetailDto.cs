namespace DotnetStore.Api.DTOs.Orders;

public sealed record OrderItemRowDto(
    int ProductId,
    string? ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string? ImageUrl = null
);

public sealed record OrderDetailDto(
    int Id,
    int CustomerUserId,
    string? CustomerEmail,
    string? CustomerName,
    int Status,
    string StatusLabel,
    string? Note,
    IReadOnlyList<OrderItemRowDto> Items,
    decimal Total,
    int? CreatedByUserId,
    int? UpdatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? ShippingFullName = null,
    string? ShippingPhone = null,
    string? ShippingCity = null,
    string? ShippingAddress = null,
    decimal ShippingFee = 0m,
    int PaymentStatus = 0,
    string PaymentStatusLabel = "",
    string? TrackingNumber = null,
    string? PaymentCard = null,
    bool CanCancel = false
);
