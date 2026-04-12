namespace DotnetStore.Api.DTOs.Products;

public sealed record ProductListItemDto(
    int Id,
    int CategoryId,
    string? CategoryName,
    string Name,
    int Stock,
    int Status,
    decimal? ActivePrice,
    string? ImageUrl);
