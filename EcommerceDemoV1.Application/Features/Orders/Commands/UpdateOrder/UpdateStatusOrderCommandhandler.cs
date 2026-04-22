using MediatR;
using EcommerceDemoV1.Domain.Entities;
using EcommerceDemoV1.Domain.Enums;
using EcommerceDemoV1.Domain.Services;

namespace EcommerceDemoV1.Application.Features.Orders.Commands.UpdateOrder;

public class UpdateStatusOrderCommandHandler : IRequestHandler<UpdateStatusOrderCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IProductVariantRepository _productVariantRepository;

    public UpdateStatusOrderCommandHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository, IUserRepository userRepository, ICurrentUserService currentUserService, IProductVariantRepository productVariantRepository)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _productVariantRepository = productVariantRepository;
    }

    public async Task<Result<string>> Handle(UpdateStatusOrderCommand request, CancellationToken cancellationToken)
    {
        var existingOrder = await _orderRepository.GetByIdAsync(request.orderId);
        if (existingOrder == null)
            return Result<string>.Failure("Order not found.");
        var oldStatus = existingOrder.Status;
        if (oldStatus == request.newStatus)
            return Result<string>.Failure("Order is already in the desired status.");

        var newOrder = await _orderRepository.UpdateOrderStatusAsync(request.orderId, request.newStatus);
        if (newOrder == null)
            return Result<string>.Failure("Failed to update order status.");

        if (newOrder.Status == OrderStatus.Completed && oldStatus != OrderStatus.Completed)
        {
            var user = await _userRepository.GetByIdAsync(existingOrder.UserId);
            if (user != null)
            {
                int earnedPoints = RankService.CalculatePoints(newOrder.FinalTotal);
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

        if (newOrder.Status == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled)
        {
            newOrder.PaymentStatus = PaymentStatus.Failed;

            var variantIds = newOrder.Items.Select(i => i.ProductVariantId).ToList();
            var variantsFromDb = await _productVariantRepository.GetListByIdsAsync(variantIds);
            var variantDict = variantsFromDb.ToDictionary(v => v.Id);

            foreach (var item in newOrder.Items)
            {
                if (variantDict.TryGetValue(item.ProductVariantId, out var variant))
                {
                    variant.StockQuantity += item.Quantity;
                    await _productVariantRepository.UpdateAsync(variant);
                }
            }
            Console.WriteLine($"[RESTOCK] Đã hoàn trả tồn kho cho đơn hàng bị hủy: {newOrder.Id}");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Order status updated successfully.");
    }
}