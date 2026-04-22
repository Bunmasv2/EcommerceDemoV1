using EcommerceDemoV1.Domain.Enums;
using MediatR;

namespace EcommerceDemoV1.Application.Features.Payment.Commands.ProcessWebhook;

public class ProcessPayOsWebhookCommandHandler : IRequestHandler<ProcessPayOsWebhookCommand, Result<string>>
{
    private readonly IPayOsService _payOsService;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessPayOsWebhookCommandHandler(IPayOsService payOsService, IOrderRepository orderRepository, IProductVariantRepository productVariantRepository, IUnitOfWork unitOfWork)
    {
        _payOsService = payOsService;
        _productVariantRepository = productVariantRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(
        ProcessPayOsWebhookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var verifiedData = await _payOsService.VerifyPaymentWebhookData(request.WebhookBody);

            // Webhook không hợp lệ / bị giả mạo
            if (verifiedData.OrderId == 0)
                return Result<string>.Success("Webhook không hợp lệ.");

            var order = await _orderRepository.GetByIdAsync(verifiedData.OrderId);
            if (order == null)
                return Result<string>.Success("Order không tồn tại.");

            // ✅ Xử lý thanh toán thành công
            if (verifiedData.IsSuccess
                && order.PaymentStatus == PaymentStatus.Pending)
            {
                order.PaymentStatus = PaymentStatus.Paid;
                order.Status = OrderStatus.Processing;

                order.Payments.Add(new EcommerceDemoV1.Domain.Entities.Payment
                {
                    Method = PaymentMethod.PayOS,
                    Amount = order.FinalTotal,
                    PaidAt = DateTime.UtcNow,
                    PayOsTransactionId = verifiedData.TransactionId,
                    Status = PaymentStatus.Paid
                });
            }
            // ✅ Xử lý hủy hoặc hết hạn
            else if ((verifiedData.IsCancelled || verifiedData.IsExpired)
                && order.PaymentStatus == PaymentStatus.Pending)
            {
                order.PaymentStatus = PaymentStatus.Failed;
                order.Status = OrderStatus.Cancelled;

                order.Payments.Add(new EcommerceDemoV1.Domain.Entities.Payment
                {
                    Method = PaymentMethod.PayOS,
                    Amount = order.FinalTotal,
                    PaidAt = DateTime.UtcNow,
                    PayOsTransactionId = verifiedData.TransactionId,
                    Status = PaymentStatus.Failed
                });

                foreach (var item in order.Items)
                {
                    if (item.ProductVariant != null)
                        item.ProductVariant.StockQuantity += item.Quantity;
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<string>.Success("Xử lý webhook thành công.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Lỗi webhook: {ex.Message}");
        }
    }
}