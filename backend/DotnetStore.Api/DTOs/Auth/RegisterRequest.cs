namespace DotnetStore.Api.DTOs.Auth;

public sealed record RegisterRequest(string Email, string Username, string Password);
