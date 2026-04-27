namespace DotnetStore.Api.DTOs.Auth;

public sealed record MeResponse(
    int Id,
    string Email,
    string UserName,
    string Role,
    DateTime? FirstLoginAt,
    DateTime? LastLoginAt,
    DateTime CreatedAt);
