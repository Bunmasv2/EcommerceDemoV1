using FluentValidation;

namespace EcommerceDemoV1.Application.Features.Shipment.Queries.GetShippingEstimate;

public class GetShippingEstimateQueryValidator : AbstractValidator<GetShippingEstimateQuery>
{
    public GetShippingEstimateQueryValidator()
    {
        RuleFor(x => x.DeliveryAddress)
            .NotEmpty().WithMessage("Địa chỉ giao hàng không được trống")
            .MinimumLength(5).WithMessage("Địa chỉ phải có ít nhất 5 ký tự");

        // RuleFor(x => x.ReceiverPhone)
        //     .NotEmpty().WithMessage("Số điện thoại người nhận không được trống")
        //     .Matches(@"^0\d{9}$").WithMessage("Số điện thoại phải bắt đầu bằng 0 và có 10 chữ số");

        // RuleFor(x => x.WeightKg)
        //     .GreaterThan(0).WithMessage("Cân nặng phải lớn hơn 0 kg")
        //     .LessThanOrEqualTo(30).WithMessage("Cân nặng không được vượt quá 30 kg");
    }
}
