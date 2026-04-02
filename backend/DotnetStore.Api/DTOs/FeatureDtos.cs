namespace DotnetStore.Api.DTOs;

public record FeatureResponseDto(
    int Id,
    string Name,
    string? DataType,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record FeatureCreateDto(string Name, string? DataType);

public record FeatureUpdateDto(string Name, string? DataType);
