using FluentValidation;

namespace EcommerceDemoV1.Application.Features.Coupons.Commands.CreateCoupon;

public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Coupon code is required.")
            .MaximumLength(50).WithMessage("Coupon code cannot exceed 50 characters.");
        RuleFor(x => x.DiscountType)
            .NotEmpty().WithMessage("Discount type is required.")
            .Must(dt => dt == "PERCENTAGE" || dt == "FIXED_AMOUNT")
            .WithMessage("Discount type must be either 'PERCENTAGE' or 'FIXED_AMOUNT'.");
        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Discount value must be greater than 0.");
        RuleFor(x => x.MinOrderValue)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum order value cannot be negative.");
        RuleFor(x => x.UsageLimit)
            .GreaterThan(0).WithMessage("Usage limit must be greater than 0.");
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");
        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");
    }
}