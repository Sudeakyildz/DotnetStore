using DotnetStore.Api.DTOs.Store;
using FluentValidation;

namespace DotnetStore.Api.Validators;

public sealed class CardRequestValidator : AbstractValidator<CardRequest>
{
    public CardRequestValidator()
    {
        RuleFor(x => x.HolderName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Number).NotEmpty().MaximumLength(25);
        RuleFor(x => x.ExpMonth).InclusiveBetween(1, 12);
        RuleFor(x => x.ExpYear).InclusiveBetween(0, 2100);
        RuleFor(x => x.Cvv).NotEmpty().Matches(@"^\d{3,4}$").WithMessage("CVV 3 veya 4 haneli olmalı.");
    }
}

public sealed class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(x => x.ShippingFullName).NotEmpty().MaximumLength(150).WithName("Ad soyad");
        RuleFor(x => x.ShippingPhone).NotEmpty().Matches(@"^[0-9+\s()\-]{10,20}$").WithMessage("Geçerli bir telefon girin.");
        RuleFor(x => x.ShippingCity).NotEmpty().MaximumLength(100).WithName("Şehir");
        RuleFor(x => x.ShippingAddress).NotEmpty().MinimumLength(10).MaximumLength(500).WithName("Adres");
        RuleFor(x => x.Note).MaximumLength(2000);
        RuleFor(x => x.Card).NotNull().SetValidator(new CardRequestValidator());
    }
}

public sealed class AddCartItemRequestValidator : AbstractValidator<AddCartItemRequest>
{
    public AddCartItemRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).InclusiveBetween(1, 20);
    }
}

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(200);
    }
}
