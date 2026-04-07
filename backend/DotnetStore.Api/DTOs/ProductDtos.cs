namespace DotnetStore.Api.DTOs;

public record ProductFeatureValueDto(int FeatureId, string FeatureName, string Value);

public record ProductListItemDto(
    int Id,
    int CategoryId,
    string? CategoryName,
    string Name,
    int Stock,
    string Status,
    decimal? ActivePrice,
    string? ImageUrl);

public record ProductDetailDto(
    int Id,
    int CategoryId,
    string? CategoryName,
    string Name,
    string? Description,
    int Stock,
    string Status,
    string? ImageUrl,
    decimal? ActivePrice,
    IReadOnlyList<ProductFeatureValueDto> FeatureValues,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ProductCreateDto(
    int CategoryId,
    string Name,
    string? Description,
    int Stock,
    string Status,
    string? ImageUrl,
    decimal Price,
    IReadOnlyList<ProductFeatureInputDto>? FeatureValues);

public record ProductFeatureInputDto(int FeatureId, string Value);

public record ProductUpdateDto(
    int CategoryId,
    string Name,
    string? Description,
    int Stock,
    string Status,
    string? ImageUrl,
    decimal? NewPrice,
    IReadOnlyList<ProductFeatureInputDto>? FeatureValues);
