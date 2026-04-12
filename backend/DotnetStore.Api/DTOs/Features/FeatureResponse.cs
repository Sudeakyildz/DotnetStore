namespace DotnetStore.Api.DTOs.Features;

public sealed record FeatureResponse(
    int Id,
    string Name,
    int DataType,
    int? CreatedByUserId,
    int? UpdatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
