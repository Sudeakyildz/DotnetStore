namespace DotnetStore.Api.DTOs.Categories;

public sealed record CategoryCreateRequest(
    string Name,
    string? Description,
    string? ImageUrl,
    string? Slug);
