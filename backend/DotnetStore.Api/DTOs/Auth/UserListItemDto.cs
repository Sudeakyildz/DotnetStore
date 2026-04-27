namespace DotnetStore.Api.DTOs.Auth;

public sealed record UserListItemDto(
    int Id,
    string Email,
    string UserName,
    string Role,
    DateTime? FirstLoginAt,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);
