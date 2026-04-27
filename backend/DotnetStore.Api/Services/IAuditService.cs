namespace DotnetStore.Api.Services;

public interface IAuditService
{
    Task LogAsync(int userId, string action, string? details, CancellationToken ct);
}
