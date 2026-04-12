using DotnetStore.Api.DTOs.Categories;
using FluentValidation;

namespace DotnetStore.Api.Validators;

public sealed class CategoryUpdateRequestValidator : AbstractValidator<CategoryUpdateRequest>
{
    public CategoryUpdateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description is not null);
        RuleFor(x => x.ImageUrl).MaximumLength(500).When(x => x.ImageUrl is not null);
        RuleFor(x => x.Slug).MaximumLength(200).When(x => x.Slug is not null);
    }
}
