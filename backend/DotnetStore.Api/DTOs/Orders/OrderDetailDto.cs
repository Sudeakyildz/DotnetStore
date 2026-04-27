namespace DotnetStore.Api.DTOs.Orders;

public sealed record OrderItemRowDto(
    int ProductId,
    string? ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
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
    DateTime UpdatedAt
);
