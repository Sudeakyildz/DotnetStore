using DotnetStore.Api.DTOs.Features;
using FluentValidation;
using StajDb.Models;

namespace DotnetStore.Api.Validators;

public sealed class FeatureCreateRequestValidator : AbstractValidator<FeatureCreateRequest>
{
    public FeatureCreateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DataType)
            .InclusiveBetween((int)FeatureDataType.None, (int)FeatureDataType.Bool);
    }
}
