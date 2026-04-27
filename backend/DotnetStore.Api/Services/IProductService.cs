using DotnetStore.Api.DTOs.Products;
using DotnetStore.Api.Services.Results;

namespace DotnetStore.Api.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductListItemDto>> SearchAsync(
        string? q,
        int? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        CancellationToken ct);

    Task<ProductDetailDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<AppResult<ProductDetailDto>> CreateAsync(ProductCreateRequest dto, CancellationToken ct);
    Task<AppResult<Unit>> UpdateAsync(int id, ProductUpdateRequest dto, CancellationToken ct);
    Task<AppResult<Unit>> UpdatePriceOnlyAsync(int id, decimal newPrice, CancellationToken ct);
    Task<AppResult<Unit>> UpdateFeatureValuesOnlyAsync(
        int id,
        IReadOnlyList<ProductFeatureValueItem> featureValues,
        CancellationToken ct);
    Task<AppResult<Unit>> DeleteAsync(int id, CancellationToken ct);
}
