using DotnetStore.Api.DTOs.Categories;
using DotnetStore.Api.Services.Results;

namespace DotnetStore.Api.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken ct);
    Task<CategoryResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<AppResult<CategoryResponse>> CreateAsync(CategoryCreateRequest dto, CancellationToken ct);
    Task<AppResult<Unit>> UpdateAsync(int id, CategoryUpdateRequest dto, CancellationToken ct);
    Task<AppResult<Unit>> DeleteAsync(int id, CancellationToken ct);
}
