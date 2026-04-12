using DotnetStore.Api.DTOs.Auth;
using DotnetStore.Api.Services.Results;

namespace DotnetStore.Api.Services;

public interface IAuthService
{
    Task<AppResult<LoginResponse>> RegisterAsync(RegisterRequest dto, CancellationToken ct);
    Task<AppResult<LoginResponse>> LoginAsync(LoginRequest dto, CancellationToken ct);
}
