using DotnetStore.Api.DTOs.Features;
using DotnetStore.Api.Services.Results;

namespace DotnetStore.Api.Services;

public interface IFeatureService
{
    Task<IReadOnlyList<FeatureResponse>> GetAllAsync(CancellationToken ct);
    Task<FeatureResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<AppResult<FeatureResponse>> CreateAsync(FeatureCreateRequest dto, CancellationToken ct);
    Task<AppResult<Unit>> UpdateAsync(int id, FeatureUpdateRequest dto, CancellationToken ct);
    Task<AppResult<Unit>> DeleteAsync(int id, CancellationToken ct);
}
