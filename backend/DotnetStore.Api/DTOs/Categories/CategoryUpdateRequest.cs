namespace DotnetStore.Api.DTOs.Categories;

public sealed record CategoryUpdateRequest(
    string Name,
    string? Description,
    string? ImageUrl,
    string? Slug);
