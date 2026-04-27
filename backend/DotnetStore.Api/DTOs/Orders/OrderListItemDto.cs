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
    string? Note
);
