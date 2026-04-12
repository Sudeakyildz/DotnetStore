using DotnetStore.Api.DTOs.Products;
using FluentValidation;
using StajDb.Models;

namespace DotnetStore.Api.Validators;

public sealed class ProductCreateRequestValidator : AbstractValidator<ProductCreateRequest>
{
    public ProductCreateRequestValidator()
    {
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(4000).When(x => x.Description is not null);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Status)
            .InclusiveBetween((int)ProductStatus.Active, (int)ProductStatus.Draft);
        RuleFor(x => x.ImageUrl).MaximumLength(500).When(x => x.ImageUrl is not null);
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .PrecisionScale(18, 2, false);
        RuleForEach(x => x.FeatureValues).ChildRules(fv =>
        {
            fv.RuleFor(v => v.FeatureId).GreaterThan(0);
            fv.RuleFor(v => v.Value).NotEmpty().MaximumLength(1000);
        }).When(x => x.FeatureValues is not null);
    }
}
