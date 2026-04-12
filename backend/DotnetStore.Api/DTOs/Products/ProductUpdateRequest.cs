namespace DotnetStore.Api.DTOs.Products;

public sealed record ProductUpdateRequest(
    int CategoryId,
    string Name,
    string? Description,
    int Stock,
    int Status,
    string? ImageUrl,
    decimal? NewPrice,
    IReadOnlyList<ProductFeatureValueItem>? FeatureValues);
