namespace DotnetStore.Api.DTOs.Products;

public sealed record ProductFeatureValuesUpdateRequest(
    IReadOnlyList<ProductFeatureValueItem> FeatureValues);
