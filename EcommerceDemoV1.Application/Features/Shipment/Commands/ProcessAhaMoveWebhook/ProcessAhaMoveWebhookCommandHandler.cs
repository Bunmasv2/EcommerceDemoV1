using MediatR;
using EcommerceDemoV1.Application.Common;
using EcommerceDemoV1.Domain.Enums;
using EcommerceDemoV1.Domain.Services;

namespace EcommerceDemoV1.Application.Features.Shipment.Commands.ProcessAhaMoveWebhook;

public class ProcessAhaMoveWebhookCommandHandler : IRequestHandler<ProcessAhaMoveWebhookCommand, Result<WebhookProcessResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IAhamoveService _ahamoveService;
    private readonly IUserRepository _userRepository;
    private readonly IProductVariantRepository _productVariantRepository;

    public ProcessAhaMoveWebhookCommandHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository, IShipmentRepository shipmentRepository, IAhamoveService ahamoveService, IUserRepository userRepository, IProductVariantRepository productVariantRepository)
    {
        _unitOfWork = unitOfWork;
        _shipmentRepository = shipmentRepository;
        _orderRepository = orderRepository;
        _ahamoveService = ahamoveService;
        _userRepository = userRepository;
        _productVariantRepository = productVariantRepository;
    }

    public async Task<Result<WebhookProcessResponse>> Handle(
        ProcessAhaMoveWebhookCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var shipment = await _shipmentRepository.GetShipmentByAhamoveOrderIdAsync(request.AhamoveOrderId);
            if (shipment == null)
            {
                return Result<WebhookProcessResponse>.Failure($"Không tìm thấy vận đơn với ID Ahamove: {request.AhamoveOrderId}");
            }

            await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            try
            {
                // Cập nhật trạng thái vận đơn
                shipment.Status = request.Status;
                shipment.UpdatedAt = DateTime.UtcNow;

                if (request.Status == "ACCEPTED" && !string.IsNullOrEmpty(request.DriverName))
                {
                    shipment.DriverName = request.DriverName;
                    shipment.DriverPhone = request.DriverPhone;
                }

                // Nếu giao hàng thành công, cập nhật trạng thái Order
                if (request.Status == "COMPLETED")
                {
                    var order = await _orderRepository.GetByIdAsync(shipment.OrderId);
                    if (order != null)
                    {
                        order.Status = OrderStatus.Completed;
                        if (order.PaymentStatus != PaymentStatus.Paid)
                        {
                            order.PaymentStatus = PaymentStatus.Paid;
                        }

                        var user = await _userRepository.GetByIdAsync(order.UserId);
                        if (user != null)
                        {
                            int earnedPoints = RankService.CalculatePoints(order.FinalTotal);
                            user.LoyaltyPoints += earnedPoints;

                            var oldRank = user.MemberRank;
                            user.MemberRank = RankService.CalculateRank(user.LoyaltyPoints);

                            if (user.MemberRank > oldRank)
                            {
                                Console.WriteLine($"[RANK UP] User {user.Id} vừa thăng hạng lên {user.MemberRank}!");
                            }

                            await _userRepository.UpdateAsync(user);
                        }
                    }
                }

                if (request.Status == "CANCELLED")
                {
                    var order = await _orderRepository.GetByIdAsync(shipment.OrderId);
                    if (order != null)
                    {
                        order.Status = OrderStatus.Cancelled;
                        if (order.PaymentStatus == PaymentStatus.Paid)
                        {
                            order.PaymentStatus = PaymentStatus.Refunding;
                        }

                        var variantIds = order.Items.Select(i => i.ProductVariantId).ToList();
                        var variantsFromDb = await _productVariantRepository.GetListByIdsAsync(variantIds);
                        var variantDict = variantsFromDb.ToDictionary(v => v.Id);

                        foreach (var item in order.Items)
                        {
                            if (variantDict.TryGetValue(item.ProductVariantId, out var variant))
                            {
                                variant.StockQuantity += item.Quantity;
                                await _productVariantRepository.UpdateAsync(variant);
                            }
                        }
                        Console.WriteLine($"[RESTOCK] Đã hoàn trả tồn kho cho đơn hàng bị hủy: {order.Id}");
                    }
                }

                // Lưu thay đổi
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return Result<WebhookProcessResponse>.Success(
                    new WebhookProcessResponse(
                        Success: true,
                        Message: $"Cập nhật trạng thái vận đơn thành công: {request.Status}",
                        OrderId: shipment.OrderId
                    )
                );
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<WebhookProcessResponse>.Failure($"Lỗi khi xử lý webhook: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            return Result<WebhookProcessResponse>.Failure($"Lỗi khi xử lý webhook: {ex.Message}");
        }
    }
}
