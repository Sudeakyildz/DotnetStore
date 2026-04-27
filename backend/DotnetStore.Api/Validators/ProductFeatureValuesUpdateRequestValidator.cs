using DotnetStore.Api.DTOs.Products;
using FluentValidation;

namespace DotnetStore.Api.Validators;

public sealed class ProductFeatureValuesUpdateRequestValidator : AbstractValidator<ProductFeatureValuesUpdateRequest>
{
    public ProductFeatureValuesUpdateRequestValidator()
    {
        RuleFor(x => x.FeatureValues).NotNull();
        RuleForEach(x => x.FeatureValues).ChildRules(fv =>
        {
            fv.RuleFor(x => x.FeatureId).GreaterThan(0);
            fv.RuleFor(x => x.Value).NotEmpty().MaximumLength(1000);
        });
    }
}
