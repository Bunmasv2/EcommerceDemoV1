using EcommerceDemoV1.Application.Common;
using EcommerceDemoV1.Domain.Common;
using EcommerceDemoV1.Domain.Enums;
using Microsoft.Extensions.Options;

using MediatR;

namespace EcommerceDemoV1.Application.Features.Payment.Commands.ProcessWebhook;

public class ProcessPayOsWebhookCommandHandler : IRequestHandler<ProcessPayOsWebhookCommand, Result<string>>
{
    private readonly IPayOsService _payOsService;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAhamoveService _ahamoveService;
    private readonly AhamoveSettings _ahamoveSettings;

    public ProcessPayOsWebhookCommandHandler(IPayOsService payOsService, IOrderRepository orderRepository, IProductVariantRepository productVariantRepository, IUnitOfWork unitOfWork, IAhamoveService ahamoveService, IOptions<AhamoveSettings> ahamoveSettings)
    {
        _payOsService = payOsService;
        _productVariantRepository = productVariantRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _ahamoveService = ahamoveService;
        _ahamoveSettings = ahamoveSettings.Value;
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

            var order = await _orderRepository.GetOrderWithDetailsAsync(verifiedData.OrderId);
            if (order == null)
                return Result<string>.Success("Order không tồn tại.");

            // Xử lý thanh toán thành công
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

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var shipment = order.Shipments.FirstOrDefault();
                if (shipment != null && string.IsNullOrEmpty(shipment.AhamoveOrderId))
                {
                    try
                    {
                        var deliveryLocation = await _ahamoveService.GeocodeAddressAsync(order.ShippingAddress);

                        var createOrderRequest = new CreateOrderRequest(
                            OrderCode: order.Id.ToString(),
                            WeightKg: 1,
                            PickupLocation: new Location(
                                Latitude: _ahamoveSettings.Latitude,
                                Longitude: _ahamoveSettings.Longitude,
                                Address: _ahamoveSettings.Address
                            ),
                            DeliveryLocation: deliveryLocation,
                            DeliveryName: order.ReceiverName,
                            DeliveryPhone: order.ReceiverPhone,
                            Note: "Gọi điện trước khi giao",
                            ServiceId: _ahamoveSettings.ServiceId,
                            CodAmount: 0
                        );

                        var ahamoveResponse = await _ahamoveService.CreateOrderAsync(createOrderRequest);
                        shipment.AhamoveOrderId = ahamoveResponse.OrderId;
                        shipment.TrackingUrl = ahamoveResponse.SharedLink;
                        shipment.Status = "IDLE";

                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        Console.WriteLine($"[AHAMOVE SUCCESS] Đã tạo vận đơn: {ahamoveResponse.OrderId}  & Tracking URL: {ahamoveResponse.SharedLink} cho Order {order.Id} sau khi thanh toán thành công.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[AHAMOVE ERROR] Lỗi tạo vận đơn cho Order {order.Id}: {ex.Message}");
                        return Result<string>.Failure($"Thanh toán thành công nhưng lỗi tạo vận đơn: {ex.Message}");
                    }
                }
            }
            //  Xử lý hủy hoặc hết hạn
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