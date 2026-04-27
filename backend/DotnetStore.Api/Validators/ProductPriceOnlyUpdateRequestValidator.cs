using DotnetStore.Api.DTOs.Products;
using FluentValidation;

namespace DotnetStore.Api.Validators;

public sealed class ProductPriceOnlyUpdateRequestValidator : AbstractValidator<ProductPriceOnlyUpdateRequest>
{
    public ProductPriceOnlyUpdateRequestValidator()
    {
        RuleFor(x => x.NewPrice).GreaterThanOrEqualTo(0);
    }
}
