namespace DotnetStore.Api.DTOs.Auth;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    int ExpiresInSeconds,
    string Email,
    string UserName,
    string Role);
