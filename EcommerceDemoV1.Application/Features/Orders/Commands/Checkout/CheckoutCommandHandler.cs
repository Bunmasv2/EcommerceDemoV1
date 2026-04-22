
using MediatR;
using EcommerceDemoV1.Domain.Entities;
using EcommerceDemoV1.Application.Common;
using EcommerceDemoV1.Domain.Services;
using EcommerceDemoV1.Domain.Enums;

namespace EcommerceDemoV1.Application.Features.Orders.Commands.Checkout;

public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, Result<CheckoutResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICartRepository _cartRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IPromotionRuleRepository _promotionRuleRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _CurrentUserService;
    private readonly IPayOsService _payOsService;

    public CheckoutCommandHandler(IUnitOfWork unitOfWork, ICartRepository cartRepository, IProductVariantRepository productVariantRepository, IPromotionRuleRepository promotionRuleRepository, ICouponRepository couponRepository, IUserRepository userRepository, IOrderRepository orderRepository, ICurrentUserService currentUserService, IPayOsService payOsService)
    {
        _unitOfWork = unitOfWork;
        _cartRepository = cartRepository;
        _productVariantRepository = productVariantRepository;
        _promotionRuleRepository = promotionRuleRepository;
        _couponRepository = couponRepository;
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _CurrentUserService = currentUserService;
        _payOsService = payOsService;
    }

    public async Task<Result<CheckoutResultDto>> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(_CurrentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated."));

        await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, cancellationToken);
        try
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null || !cart.Items.Any())
                return Result<CheckoutResultDto>.Failure("Cart is empty.");

            var user = await _userRepository.GetByIdAsync(userId);

            // 1. Chạy Promotion Engine
            var activePromotionRules = await _promotionRuleRepository.GetActivePromotionRulesAsync(DateTime.UtcNow);
            var engine = new PromotionEngine();
            var promotionResult = engine.ApplyBestRule(cart.Items, activePromotionRules);

            // 2. GOM TOÀN BỘ ID SẢN PHẨM (Hàng mua + Quà tặng) ĐỂ QUERY 1 LẦN DUY NHẤT
            var variantIdsToFetch = cart.Items.Select(i => i.ProductVariantId)
                .Union(promotionResult.Gifts.Select(g => g.ProductVariantId))
                .Distinct()
                .ToList();

            var variantsFromDb = await _productVariantRepository.GetListByIdsAsync(variantIdsToFetch);

            // Chuyển thành Dictionary để tra cứu trong RAM
            var variantDict = variantsFromDb.ToDictionary(v => v.Id);

            var inventoryToDeduct = new Dictionary<int, int>();
            decimal subTotal = 0;

            // 3. TÍNH TIỀN & GOM LƯỢNG TỒN KHO CẦN TRỪ
            foreach (var item in cart.Items)
            {
                if (!variantDict.TryGetValue(item.ProductVariantId, out var productVariant))
                    return Result<CheckoutResultDto>.Failure($"Product variant with ID {item.ProductVariantId} not found in DB.");

                if (inventoryToDeduct.ContainsKey(item.ProductVariantId))
                    inventoryToDeduct[item.ProductVariantId] += item.Quantity;
                else
                    inventoryToDeduct.Add(item.ProductVariantId, item.Quantity);

                subTotal += productVariant.Price * item.Quantity;
            }

            foreach (var gift in promotionResult.Gifts)
            {
                if (inventoryToDeduct.ContainsKey(gift.ProductVariantId))
                    inventoryToDeduct[gift.ProductVariantId] += gift.Quantity;
                else
                    inventoryToDeduct.Add(gift.ProductVariantId, gift.Quantity);
            }

            // 4. KIỂM TRA VÀ TRỪ TỒN KHO LẦN CUỐI
            foreach (var kvp in inventoryToDeduct)
            {
                var variantId = kvp.Key;
                var totalQuantityNeeded = kvp.Value;

                var productVariant = variantDict[variantId];

                if (productVariant.StockQuantity < totalQuantityNeeded)
                    return Result<CheckoutResultDto>.Failure($"Not enough stock for product variant {productVariant.Id}. Need: {totalQuantityNeeded}, Have: {productVariant.StockQuantity}");

                productVariant.StockQuantity -= totalQuantityNeeded;
            }

            // 5. TÍNH TOÁN GIÁ TRỊ TỔNG (Coupon, Rank)
            decimal totalAfterPromotion = subTotal - promotionResult.TotalDiscount;
            if (totalAfterPromotion < 0) totalAfterPromotion = 0;

            decimal rankDiscountRate = RankService.GetDiscountRate(user.MemberRank);
            decimal rankDiscountAmount = totalAfterPromotion * rankDiscountRate;
            decimal totalAfterRank = totalAfterPromotion - rankDiscountAmount;
            if (totalAfterRank < 0) totalAfterRank = 0;

            decimal couponDiscountAmount = 0;
            Coupon? appliedCoupon = null;

            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                appliedCoupon = await _couponRepository.GetValidCouponByCodeAsync(request.CouponCode, DateTime.UtcNow);
                if (appliedCoupon == null || appliedCoupon.UsageLimit <= 0)
                    return Result<CheckoutResultDto>.Failure("Invalid or expired coupon code.");

                if (totalAfterRank < appliedCoupon.MinOrderValue)
                    return Result<CheckoutResultDto>.Failure($"Order total must be at least {appliedCoupon.MinOrderValue} to use this coupon.");

                appliedCoupon.UsageLimit -= 1;
                couponDiscountAmount = appliedCoupon.DiscountType == "PERCENTAGE"
                    ? totalAfterRank * (appliedCoupon.Value / 100)
                    : appliedCoupon.Value;
            }

            decimal finalTotal = totalAfterRank - couponDiscountAmount;
            if (finalTotal < 0) finalTotal = 0;

            // 6. TẠO ORDER ITEMS
            var orderItems = new List<OrderItem>();

            foreach (var item in cart.Items)
            {
                orderItems.Add(new OrderItem
                {
                    ProductVariantId = item.ProductVariantId,
                    Quantity = item.Quantity,
                    UnitPrice = variantDict[item.ProductVariantId].Price
                });
            }

            foreach (var gift in promotionResult.Gifts)
            {
                orderItems.Add(new OrderItem
                {
                    ProductVariantId = gift.ProductVariantId,
                    Quantity = gift.Quantity,
                    UnitPrice = 0
                });
            }

            var initialOrderStatus = finalTotal == 0 ? OrderStatus.Processing : OrderStatus.Pending;
            var initialPaymentStatus = finalTotal == 0 ? PaymentStatus.Paid : PaymentStatus.Pending;

            // 7. LƯU ĐƠN HÀNG
            var order = new Order
            {
                UserId = userId,
                ShippingAddress = request.ShippingAddress,
                Status = initialOrderStatus,
                PaymentStatus = initialPaymentStatus,
                SubTotal = subTotal,
                PromotionDiscount = promotionResult.TotalDiscount,
                RankDiscount = rankDiscountAmount,
                CouponDiscount = couponDiscountAmount,
                CouponCode = appliedCoupon?.Code,
                FinalTotal = finalTotal,
                Items = orderItems
            };

            var payment = new EcommerceDemoV1.Domain.Entities.Payment
            {
                Amount = finalTotal,
                Method = finalTotal == 0 ? PaymentMethod.COD : request.PaymentMethod,
                Status = PaymentStatus.Pending
            };

            order.Payments.Add(payment);

            await _orderRepository.CreateOrderAsync(order);

            await _cartRepository.DeleteCartAsync(cart.Id);

            // SAVE Order mới, update Tồn kho, update Lượt Coupon, Xóa Cart
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            string? paymentUrl = null;

            if (request.PaymentMethod == PaymentMethod.PayOS)
            {
                try
                {
                    paymentUrl = await _payOsService.CreatePaymentAsync(order, payment);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<CheckoutResultDto>.Failure($"Lỗi tạo cổng thanh toán PayOS: {ex.Message}");
                }
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<CheckoutResultDto>.Success(new CheckoutResultDto
            {
                OrderId = order.Id,
                Message = "Order created successfully.",
                PaymentUrl = paymentUrl
            });
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<CheckoutResultDto>.Failure("An error occurred during checkout. Please try again.");
        }
    }
}