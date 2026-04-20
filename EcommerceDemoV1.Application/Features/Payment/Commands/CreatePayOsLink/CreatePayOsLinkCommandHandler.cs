
using MediatR;
using EcommerceDemoV1.Domain.Entities;
using EcommerceDemoV1.Domain.Enums;

namespace EcommerceDemoV1.Application.Features.Payment.Commands.CreatePayOsLink;

public record CreatePayOsLinkCommandHandler : IRequestHandler<CreatePayOsLinkCommand, Result<string>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPayOsService _payOsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePayOsLinkCommandHandler(IOrderRepository orderRepository, IPayOsService payOsService, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _payOsService = payOsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(CreatePayOsLinkCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.orderId);
        if (order == null)
            return Result<string>.Failure("Order not found.");

        if (order.PaymentStatus == PaymentStatus.Paid)
            return Result<string>.Failure("Đơn hàng này đã được thanh toán.");

        if (order.PaymentStatus != PaymentStatus.Pending)
            return Result<string>.Failure("Order is not pending payment.");

        var payment = new EcommerceDemoV1.Domain.Entities.Payment
        {
            OrderId = order.Id,
            Amount = order.FinalTotal,
            Method = PaymentMethod.PayOS,
            Status = PaymentStatus.Pending,
        };
        string paymentUrl = await _payOsService.CreatePaymentAsync(order, payment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<string>.Success(paymentUrl);
    }
}