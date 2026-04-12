namespace DotnetStore.Api.DTOs.Products;

public sealed record ProductCreateRequest(
    int CategoryId,
    string Name,
    string? Description,
    int Stock,
    int Status,
    string? ImageUrl,
    decimal Price,
    IReadOnlyList<ProductFeatureValueItem>? FeatureValues);
