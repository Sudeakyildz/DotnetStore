namespace DotnetStore.Api.DTOs.Products;

public sealed record ProductDetailDto(
    int Id,
    int CategoryId,
    string? CategoryName,
    string Name,
    string? Description,
    int Stock,
    int Status,
    string? ImageUrl,
    decimal? ActivePrice,
    IReadOnlyList<ProductFeatureValueDto> FeatureValues,
    int? CreatedByUserId,
    int? UpdatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
