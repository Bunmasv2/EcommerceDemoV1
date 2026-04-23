using FluentValidation;
using EcommerceDemoV1.Domain.Enums;

namespace EcommerceDemoV1.Application.Features.Orders.Commands.Checkout;

public class CheckoutCommandValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutCommandValidator()
    {
        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Địa chỉ giao hàng không được để trống.")
            .MaximumLength(300).WithMessage("Địa chỉ giao hàng không được vượt quá 300 ký tự.");


        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Phương thức thanh toán không hợp lệ.");

        RuleFor(x => x.CouponCode)
            .MaximumLength(50).WithMessage("Mã giảm giá không được vượt quá 50 ký tự.")
            // Dùng .When() để báo cho FluentValidation biết: "Chỉ chạy dòng kiểm tra ở trên NẾU CouponCode không bị rỗng"
            .When(x => !string.IsNullOrWhiteSpace(x.CouponCode));
    }
}