using FluentValidation;
namespace EcommerceDemoV1.Application.Features.Orders.Queries.GetOrders;

public class GetOrderCommandValidator : AbstractValidator<GetOrderCommand>
{
    public GetOrderCommandValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithMessage("Page must be greater than 0.");
        RuleFor(x => x.Size).GreaterThan(0).WithMessage("Size must be greater than 0.");
    }
}