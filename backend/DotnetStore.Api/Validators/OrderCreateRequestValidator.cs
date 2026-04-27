using DotnetStore.Api.DTOs.Orders;
using FluentValidation;

namespace DotnetStore.Api.Validators;

public class OrderCreateRequestValidator : AbstractValidator<OrderCreateRequest>
{
    public OrderCreateRequestValidator()
    {
        RuleFor(x => x.CustomerUserId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new OrderLineItemValidator());
        RuleFor(x => x.Note).MaximumLength(2000).When(x => x.Note is not null);
    }
}

public class OrderLineItemValidator : AbstractValidator<OrderLineItem>
{
    public OrderLineItemValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).InclusiveBetween(1, 9999);
    }
}
