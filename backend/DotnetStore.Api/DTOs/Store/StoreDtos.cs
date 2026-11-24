namespace DotnetStore.Api.DTOs.Store;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

public sealed record StoreProductListItem(
    int Id,
    string Name,
    int CategoryId,
    string? CategoryName,
    decimal Price,
    string? ImageUrl,
    int Stock);

public sealed record StoreFeatureValue(string Name, string Value);

public sealed record StoreProductDetail(
    int Id,
    string Name,
    string? Description,
    int CategoryId,
    string? CategoryName,
    decimal Price,
    string? ImageUrl,
    int Stock,
    IReadOnlyList<StoreFeatureValue> Features,
    IReadOnlyList<StoreProductListItem> Related);

public sealed record StoreCategoryDto(int Id, string Name, string? Slug, string? Description, string? ImageUrl, int ProductCount);

public sealed record CartItemDto(
    int ProductId,
    string Name,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    int Stock,
    decimal LineTotal,
    bool Available);

public sealed record CartDto(
    IReadOnlyList<CartItemDto> Items,
    int ItemCount,
    decimal Subtotal,
    decimal ShippingFee,
    decimal Total,
    decimal FreeShippingThreshold);

public sealed record AddCartItemRequest(int ProductId, int Quantity);
public sealed record SetCartQuantityRequest(int Quantity);

public sealed record CardRequest(string HolderName, string Number, int ExpMonth, int ExpYear, string Cvv);

public sealed record CheckoutRequest(
    string ShippingFullName,
    string ShippingPhone,
    string ShippingCity,
    string ShippingAddress,
    string? Note,
    CardRequest Card);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
