using DotnetStore.Api.DTOs.Orders;
using FluentValidation;
using StajDb.Models;

namespace DotnetStore.Api.Validators;

public class OrderStatusUpdateRequestValidator : AbstractValidator<OrderStatusUpdateRequest>
{
    public OrderStatusUpdateRequestValidator()
    {
        RuleFor(x => x.Status)
            .InclusiveBetween((int)OrderStatus.BegeniyeEklendi, (int)OrderStatus.IptalEdildi);
    }
}
