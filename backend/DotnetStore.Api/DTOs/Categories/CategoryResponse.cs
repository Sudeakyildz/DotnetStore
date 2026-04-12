namespace DotnetStore.Api.DTOs.Categories;

public sealed record CategoryResponse(
    int Id,
    string Name,
    string? Description,
    string? ImageUrl,
    string? Slug,
    int? CreatedByUserId,
    int? UpdatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
