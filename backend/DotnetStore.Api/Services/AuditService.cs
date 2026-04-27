using StajDb;
using StajDb.Models;

namespace DotnetStore.Api.Services;

public sealed class AuditService : IAuditService
{
    private readonly DataContext _db;

    public AuditService(DataContext db)
    {
        _db = db;
    }

    public async Task LogAsync(int userId, string action, string? details, CancellationToken ct)
    {
        if (details is { Length: > 1000 })
            details = details[..1000];

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            Details = details,
            CreatedAtUtc = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync(ct);
    }
}
