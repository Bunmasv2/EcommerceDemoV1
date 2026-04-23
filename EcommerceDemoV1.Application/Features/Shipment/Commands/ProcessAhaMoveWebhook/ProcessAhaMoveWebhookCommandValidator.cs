using FluentValidation;

namespace EcommerceDemoV1.Application.Features.Shipment.Commands.ProcessAhaMoveWebhook;

public class ProcessAhaMoveWebhookCommandValidator : AbstractValidator<ProcessAhaMoveWebhookCommand>
{
    public ProcessAhaMoveWebhookCommandValidator()
    {
        RuleFor(x => x.AhamoveOrderId)
            .NotEmpty().WithMessage("Mã đơn hàng Ahamove không được trống");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Trạng thái không được trống")
            .Must(x => new[] { "IDLE", "ACCEPTED", "IN_PROCESS", "COMPLETED", "CANCELLED" }.Contains(x))
            .WithMessage("Trạng thái không hợp lệ");

        RuleFor(x => x.DriverName)
            .NotEmpty().When(x => x.Status == "ACCEPTED")
            .WithMessage("Tên tài xế là bắt buộc khi trạng thái là ACCEPTED");

        RuleFor(x => x.DriverPhone)
            .NotEmpty().When(x => x.Status == "ACCEPTED")
            .WithMessage("Số điện thoại tài xế là bắt buộc khi trạng thái là ACCEPTED");
    }
}
