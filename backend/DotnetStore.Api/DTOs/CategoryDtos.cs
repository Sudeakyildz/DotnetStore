namespace DotnetStore.Api.DTOs;

public record CategoryResponseDto(
    int Id,
    string Name,
    string? Description,
    string? ImageUrl,
    string? Slug,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CategoryCreateDto(
    string Name,
    string? Description,
    string? ImageUrl,
    string? Slug);

public record CategoryUpdateDto(
    string Name,
    string? Description,
    string? ImageUrl,
    string? Slug);
