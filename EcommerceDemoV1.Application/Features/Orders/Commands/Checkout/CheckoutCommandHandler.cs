
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
        var userId = int.Parse(_CurrentUserService.UserId);
        if (string.IsNullOrEmpty(userId.ToString()))
            return Result<CheckoutResultDto>.Failure("User is not authenticated.");

        await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, cancellationToken);
        try
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null || !cart.Items.Any())
                return Result<CheckoutResultDto>.Failure("Cart is empty.");

            var user = await _userRepository.GetByIdAsync(userId);

            // 1. CHẠY BỘ MÁY KHUYẾN MÃI TRƯỚC (Để biết có được tặng quà gì không)
            var activePromotionRules = await _promotionRuleRepository.GetActivePromotionRulesAsync(DateTime.UtcNow);
            var engine = new PromotionEngine();
            var promotionResult = engine.ApplyBestRule(cart.Items, activePromotionRules);

            // 2. GOM TẤT CẢ HÀNG CẦN XUẤT KHO (Hàng mua + Quà tặng) vào Dictionary
            var inventoryToDeduct = new Dictionary<int, int>();
            decimal subTotal = 0;

            // 2.1. Quét hàng khách mua
            foreach (var item in cart.Items)
            {
                if (inventoryToDeduct.ContainsKey(item.ProductVariantId))
                    inventoryToDeduct[item.ProductVariantId] += item.Quantity;
                else
                    inventoryToDeduct.Add(item.ProductVariantId, item.Quantity);

                // Lấy giá chuẩn từ ProductVariant (Nên lấy từ DB thay vì Cart để tránh lệch giá khi có biến động)
                var productVariant = await _productVariantRepository.GetByIdAsync(item.ProductVariantId);
                subTotal += productVariant!.Price * item.Quantity;
            }

            // 2.2. Quét quà khách được tặng
            foreach (var gift in promotionResult.Gifts)
            {
                if (inventoryToDeduct.ContainsKey(gift.ProductVariantId))
                    inventoryToDeduct[gift.ProductVariantId] += gift.Quantity;
                else
                    inventoryToDeduct.Add(gift.ProductVariantId, gift.Quantity);
            }

            // 3. KIỂM TRA VÀ TRỪ TỒN KHO LẦN CUỐI (Khóa dòng - RepeatableRead)
            foreach (var kvp in inventoryToDeduct)
            {
                var variantId = kvp.Key;
                var totalQuantityNeeded = kvp.Value;

                var productVariant = await _productVariantRepository.GetByIdAsync(variantId);
                if (productVariant == null)
                    return Result<CheckoutResultDto>.Failure($"Product variant with ID {variantId} not found.");

                if (productVariant.StockQuantity < totalQuantityNeeded)
                    return Result<CheckoutResultDto>.Failure($"Not enough stock for product variant {productVariant.Id}. Need: {totalQuantityNeeded}, Have: {productVariant.StockQuantity}");

                // Trừ kho
                productVariant.StockQuantity -= totalQuantityNeeded;
            }

            // 4. TÍNH TOÁN GIÁ TRỊ (Coupon, Rank)
            decimal totalAfterPromotion = subTotal - promotionResult.TotalDiscount;
            decimal rankDiscountRate = RankService.GetDiscountRate(user.MemberRank);
            decimal rankDiscountAmount = totalAfterPromotion * rankDiscountRate;
            decimal totalAfterRank = totalAfterPromotion - rankDiscountAmount;

            decimal couponDiscountAmount = 0;
            Coupon? appliedCoupon = null;
            // Apply coupon if provided
            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                appliedCoupon = await _couponRepository.GetValidCouponByCodeAsync(request.CouponCode, DateTime.UtcNow);
                if (appliedCoupon == null || appliedCoupon.UsageLimit <= 0)
                    return Result<CheckoutResultDto>.Failure("Invalid or expired coupon code.");

                if (totalAfterRank < appliedCoupon.MinOrderValue)
                    return Result<CheckoutResultDto>.Failure($"Order total must be at least {appliedCoupon.MinOrderValue} to use this coupon.");

                appliedCoupon = appliedCoupon;
                if (appliedCoupon.DiscountType == "PERCENTAGE")
                {
                    couponDiscountAmount = totalAfterRank * (appliedCoupon.Value / 100);
                }
                else if (appliedCoupon.DiscountType == "FIXED_AMOUNT")
                {
                    couponDiscountAmount = appliedCoupon.Value;
                }

                appliedCoupon.UsageLimit -= 1;
            }

            decimal finalTotal = totalAfterRank - couponDiscountAmount;

            var orderItems = new List<OrderItem>();

            // Hàng mua
            foreach (var item in cart.Items)
            {
                var variant = await _productVariantRepository.GetByIdAsync(item.ProductVariantId);
                orderItems.Add(new OrderItem
                {
                    ProductVariantId = item.ProductVariantId,
                    Quantity = item.Quantity,
                    UnitPrice = variant!.Price
                });
            }

            // Quà tặng
            foreach (var gift in promotionResult.Gifts)
            {
                orderItems.Add(new OrderItem
                {
                    ProductVariantId = gift.ProductVariantId,
                    Quantity = gift.Quantity,
                    UnitPrice = 0
                });
            }

            // Create order
            var order = new Order
            {
                UserId = userId,
                ShippingAddress = request.ShippingAddress,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                SubTotal = subTotal,
                PromotionDiscount = promotionResult.TotalDiscount,
                RankDiscount = rankDiscountAmount,
                CouponDiscount = couponDiscountAmount,
                CouponCode = appliedCoupon?.Code,
                FinalTotal = finalTotal,
                Items = orderItems
            };

            await _orderRepository.CreateOrderAsync(order);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            string? paymentUrl = null;
            string returnMessage = "Order created successfully.";
            if (request.PaymentMethod == PaymentMethod.PayOS)
            {
                var payment = new Payment
                {
                    OrderId = order.Id,
                    Amount = finalTotal,
                    Method = PaymentMethod.PayOS,
                    Status = PaymentStatus.Pending
                };

                paymentUrl = await _payOsService.CreatePaymentAsync(order, payment);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                returnMessage = "Please complete the payment.";
            }

            _cartRepository.DeleteCartAsync(cart.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result<CheckoutResultDto>.Success(new CheckoutResultDto
            {
                OrderId = order.Id,
                Message = returnMessage,
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