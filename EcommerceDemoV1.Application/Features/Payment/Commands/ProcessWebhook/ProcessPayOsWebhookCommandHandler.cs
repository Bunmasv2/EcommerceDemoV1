using EcommerceDemoV1.Domain.Enums;
using MediatR;

namespace EcommerceDemoV1.Application.Features.Payment.Commands.ProcessWebhook;

public class ProcessPayOsWebhookCommandHandler : IRequestHandler<ProcessPayOsWebhookCommand, Result<string>>
{
    private readonly IPayOsService _payOsService;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessPayOsWebhookCommandHandler(IPayOsService payOsService, IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _payOsService = payOsService;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(ProcessPayOsWebhookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Xác thực dữ liệu
            var verifiedData = _payOsService.VerifyPaymentWebhookData(request.WebhookBody);

            if (verifiedData.IsSuccess)
            {
                var orderId = (int)verifiedData.OrderId;
                var order = await _orderRepository.GetByIdAsync(orderId);

                if (order != null && order.PaymentStatus == PaymentStatus.Pending)
                {
                    // 3. Cập nhật trạng thái đơn hàng
                    order.PaymentStatus = PaymentStatus.Paid;
                    order.Status = OrderStatus.Processing; // Đã trả tiền thì chuyển sang chờ giao hàng

                    // 4. Lưu lại lịch sử thanh toán
                    order.Payments.Add(new EcommerceDemoV1.Domain.Entities.Payment
                    {
                        Method = PaymentMethod.PayOS,
                        Amount = order.FinalTotal,
                        PaidAt = DateTime.UtcNow,
                        PayOsTransactionId = verifiedData.TransactionId,
                        Status = PaymentStatus.Paid
                    });

                    // 5. Lưu xuống DB
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                }
            }
            return Result<string>.Success("Xử lý webhook thành công");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Lỗi webhook: {ex.Message}");
        }
    }
}
