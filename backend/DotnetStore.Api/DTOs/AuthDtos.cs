namespace DotnetStore.Api.DTOs;

public record LoginRequestDto(string Username, string Password);

public record LoginResponseDto(string AccessToken, string TokenType, int ExpiresInSeconds);
