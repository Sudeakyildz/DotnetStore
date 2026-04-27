namespace DotnetStore.Api.DTOs.Auth;

public sealed record AuditLogListItemDto(
    int Id,
    string UserEmail,
    string UserName,
    string Action,
    string? Details,
    DateTime CreatedAtUtc);
