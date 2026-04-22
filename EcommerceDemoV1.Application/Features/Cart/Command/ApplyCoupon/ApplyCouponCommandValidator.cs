namespace EcommerceDemoV1.Application.Features.Cart.Commands.ApplyCoupon;

using FluentValidation;

public class ApplyCouponCommandValidator : AbstractValidator<ApplyCouponCommand>
{
    public ApplyCouponCommandValidator()
    {
        RuleFor(x => x.couponCode)
            .NotEmpty().WithMessage("Coupon code is required.");
    }
}