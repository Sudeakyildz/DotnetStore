namespace DotnetStore.Api.DTOs.Orders;

public sealed record OrderLineItem(int ProductId, int Quantity);

public sealed record OrderCreateRequest(
    int CustomerUserId,
    IReadOnlyList<OrderLineItem> Items,
    string? Note,
    int? Status
);
