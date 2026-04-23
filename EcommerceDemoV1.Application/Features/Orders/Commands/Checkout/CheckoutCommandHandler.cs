
using MediatR;
using EcommerceDemoV1.Domain.Entities;
using EcommerceDemoV1.Application.Common;
using EcommerceDemoV1.Domain.Services;
using EcommerceDemoV1.Domain.Enums;
using Microsoft.Extensions.Options;
using EcommerceDemoV1.Domain.Common;

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

    private readonly IAhamoveService _ahamoveService;
    private readonly AhamoveSettings _ahamoveSettings;

    public CheckoutCommandHandler(IUnitOfWork unitOfWork, ICartRepository cartRepository, IProductVariantRepository productVariantRepository, IPromotionRuleRepository promotionRuleRepository, ICouponRepository couponRepository, IUserRepository userRepository, IOrderRepository orderRepository, ICurrentUserService currentUserService, IPayOsService payOsService, IAhamoveService ahamoveService, IOptions<AhamoveSettings> ahamoveSettings)
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
        _ahamoveService = ahamoveService;
        _ahamoveSettings = ahamoveSettings.Value;
    }

    public async Task<Result<CheckoutResultDto>> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(_CurrentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated."));
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart == null || !cart.Items.Any())
            return Result<CheckoutResultDto>.Failure("Cart is empty.");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Result<CheckoutResultDto>.Failure($"Không tìm thấy User với ID = {userId} trong Database. Bác kiểm tra lại Token nhé!");

        //  GỌI EXTERNAL API CỦA AHAMOVE BÊN NGOÀI TRANSACTION
        var pickupLocation = new Location(
            Latitude: _ahamoveSettings.Latitude,
            Longitude: _ahamoveSettings.Longitude,
            Address: _ahamoveSettings.Address
        );

        Location deliveryLocation;
        EstimateResponse estimateResponse;
        try
        {
            deliveryLocation = await _ahamoveService.GeocodeAddressAsync(request.ShippingAddress);
            estimateResponse = await _ahamoveService.EstimateOrderFeeAsync(
                pickupLocation: pickupLocation,
                deliveryLocation: deliveryLocation,
                weightKg: 1,
                serviceId: _ahamoveSettings.ServiceId
            );
        }
        catch (Exception ex)
        {
            return Result<CheckoutResultDto>.Failure($"Lỗi tính phí giao hàng: {ex.Message}");
        }

        decimal finalShippingFee = estimateResponse.TotalPay;

        Order? order = null;
        EcommerceDemoV1.Domain.Entities.Payment? payment = null;

        await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, cancellationToken);
        try
        {
            // 1. Chạy Promotion Engine
            var activePromotionRules = await _promotionRuleRepository.GetActivePromotionRulesAsync(DateTime.UtcNow);
            if (activePromotionRules == null)
                activePromotionRules = new List<PromotionRule>();

            var engine = new PromotionEngine();
            var promotionResult = engine.ApplyBestRule(cart.Items, activePromotionRules);
            if (promotionResult == null)
                return Result<CheckoutResultDto>.Failure("Cỗ máy PromotionEngine đang trả về kết quả rỗng (null).");
            if (promotionResult.Gifts == null)
                promotionResult.Gifts = new List<GiftItem>();

            //GOM TOÀN BỘ ID SẢN PHẨM (Hàng mua + Quà tặng) ĐỂ QUERY 1 LẦN DUY NHẤT
            var variantIdsToFetch = cart.Items.Select(i => i.ProductVariantId)
                .Union(promotionResult.Gifts.Select(g => g.ProductVariantId))
                .Distinct()
                .ToList();

            var variantsFromDb = await _productVariantRepository.GetListByIdsAsync(variantIdsToFetch);

            // Chuyển thành Dictionary để tra cứu trong RAM
            var variantDict = variantsFromDb.ToDictionary(v => v.Id);

            var inventoryToDeduct = new Dictionary<int, int>();
            decimal subTotal = 0;

            //TÍNH TIỀN & GOM LƯỢNG TỒN KHO CẦN TRỪ
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

            // KIỂM TRA VÀ TRỪ TỒN KHO LẦN CUỐI
            foreach (var kvp in inventoryToDeduct)
            {
                var variantId = kvp.Key;
                var totalQuantityNeeded = kvp.Value;

                var productVariant = variantDict[variantId];

                if (productVariant.StockQuantity < totalQuantityNeeded)
                    return Result<CheckoutResultDto>.Failure($"Not enough stock for product variant {productVariant.Id}. Need: {totalQuantityNeeded}, Have: {productVariant.StockQuantity}");

                productVariant.StockQuantity -= totalQuantityNeeded;
            }

            // TÍNH TOÁN GIÁ TRỊ TỔNG (Coupon, Rank)
            decimal totalAfterPromotion = Math.Max(0, subTotal - promotionResult.TotalDiscount);
            decimal rankDiscountRate = RankService.GetDiscountRate(user.MemberRank);
            decimal rankDiscountAmount = totalAfterPromotion * rankDiscountRate;
            decimal totalAfterRank = Math.Max(0, totalAfterPromotion - rankDiscountAmount);

            decimal couponDiscountAmount = 0;
            Coupon? appliedCoupon = null;

            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                appliedCoupon = await _couponRepository.GetValidCouponByCodeAsync(request.CouponCode, DateTime.UtcNow);
                if (appliedCoupon == null || appliedCoupon.UsageLimit <= 0)
                    return Result<CheckoutResultDto>.Failure("Mã giảm giá không hợp lệ hoặc đã hết hạn.");

                if (totalAfterRank < appliedCoupon.MinOrderValue)
                    return Result<CheckoutResultDto>.Failure($"Đơn hàng phải tối thiểu {appliedCoupon.MinOrderValue} để dùng mã này.");

                appliedCoupon.UsageLimit -= 1;
                couponDiscountAmount = appliedCoupon.DiscountType == "PERCENTAGE"
                    ? totalAfterRank * (appliedCoupon.Value / 100) : appliedCoupon.Value;
            }

            decimal CODTotal = Math.Max(0, totalAfterRank - couponDiscountAmount);

            decimal finalTotal = CODTotal + finalShippingFee;

            //TẠO ORDER ITEMS
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
            order = new Order
            {
                UserId = userId,
                ShippingAddress = request.ShippingAddress,
                ReceiverName = request.ReceiverName ?? user.FullName,
                ReceiverPhone = request.ReceiverPhone,
                Status = initialOrderStatus,
                PaymentStatus = initialPaymentStatus,
                SubTotal = subTotal,
                PromotionDiscount = promotionResult.TotalDiscount,
                RankDiscount = rankDiscountAmount,
                CouponDiscount = couponDiscountAmount,
                CouponCode = appliedCoupon?.Code,
                ShippingFee = finalShippingFee,
                FinalTotal = finalTotal,
                Items = orderItems
            };

            payment = new EcommerceDemoV1.Domain.Entities.Payment
            {
                Amount = finalTotal,
                Method = finalTotal == 0 ? PaymentMethod.COD : request.PaymentMethod,
                Status = PaymentStatus.Pending
            };

            if (payment == null)
                return Result<CheckoutResultDto>.Failure("Payment object is null.");

            order.Payments.Add(payment);

            var shipment = new EcommerceDemoV1.Domain.Entities.Shipment
            {
                ServiceId = _ahamoveSettings.ServiceId,
                Status = "IDLE",
                ShippingFee = finalShippingFee,
                Distance = estimateResponse.Distance
            };
            order.Shipments.Add(shipment);

            await _orderRepository.CreateOrderAsync(order);

            await _cartRepository.DeleteCartAsync(cart.Id);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<CheckoutResultDto>.Failure($"NRE ở dòng nào: {ex.StackTrace}");
        }

        string? paymentUrl = null;
        string warningMessage = "";
        if (request.PaymentMethod == PaymentMethod.COD || order.FinalTotal == 0)
        {
            try
            {
                var createOrderRequest = new CreateOrderRequest(
                    OrderCode: order.Id.ToString(),
                    WeightKg: 1,
                    PickupLocation: pickupLocation,
                    DeliveryLocation: deliveryLocation,
                    DeliveryName: order.ReceiverName,
                    DeliveryPhone: order.ReceiverPhone,
                    Note: "Gọi điện trước khi giao",
                    ServiceId: _ahamoveSettings.ServiceId,
                    CodAmount: (int)order.FinalTotal
                );

                var ahamoveResponse = await _ahamoveService.CreateOrderAsync(createOrderRequest);

                var shipment = order.Shipments.First();
                shipment.AhamoveOrderId = ahamoveResponse.OrderId;
                shipment.TrackingUrl = ahamoveResponse.SharedLink;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<CheckoutResultDto>.Failure($"Lỗi tạo đơn vận chuyển AhaMove: {ex.Message}");
            }
        }


        if (request.PaymentMethod == PaymentMethod.PayOS)
        {
            try
            {
                paymentUrl = await _payOsService.CreatePaymentAsync(order, payment);
            }
            catch (Exception ex)
            {
                return Result<CheckoutResultDto>.Success(new CheckoutResultDto
                {
                    OrderId = order.Id,
                    Message = $"Đơn hàng đã tạo thành công nhưng hệ thống PayOS đang gián đoạn. Vui lòng thanh toán sau. Lỗi: {ex.Message}"
                });
            }
        }
        return Result<CheckoutResultDto>.Success(new CheckoutResultDto
        {
            OrderId = order.Id,
            Message = "Order created successfully.",
            PaymentUrl = paymentUrl
        });
    }
}