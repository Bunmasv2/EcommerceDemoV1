using FluentValidation;

namespace EcommerceDemoV1.Application.Features.Orders.Commands.UpdateOrder;

public class UpdateStatusOrderCommandValidator : AbstractValidator<UpdateStatusOrderCommand>
{
    public UpdateStatusOrderCommandValidator()
    {
        RuleFor(x => x.orderId).GreaterThan(0).WithMessage("Order ID must be greater than 0");
        RuleFor(x => x.newStatus).IsInEnum().WithMessage("Invalid order status");
    }
}