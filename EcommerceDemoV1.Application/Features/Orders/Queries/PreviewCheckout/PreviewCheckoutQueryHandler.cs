using MediatR;
using EcommerceDemoV1.Application.Common;
using EcommerceDemoV1.Domain.Entities;
using EcommerceDemoV1.Domain.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace EcommerceDemoV1.Application.Features.Orders.Queries.PreviewCheckout;

public class PreviewCheckoutQueryHandler : IRequestHandler<PreviewCheckoutQuery, Result<PreviewCheckoutDto>>
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IPromotionRuleRepository _promotionRuleRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public PreviewCheckoutQueryHandler(
        ICartRepository cartRepository,
        IProductVariantRepository productVariantRepository,
        IPromotionRuleRepository promotionRuleRepository,
        ICouponRepository couponRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService)
    {
        _cartRepository = cartRepository;
        _productVariantRepository = productVariantRepository;
        _promotionRuleRepository = promotionRuleRepository;
        _couponRepository = couponRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PreviewCheckoutDto>> Handle(PreviewCheckoutQuery request, CancellationToken cancellationToken)
    {
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            return Result<PreviewCheckoutDto>.Failure("User is not authenticated.");

        var cart = await _cartRepository.GetCartByUserIdAsync(userId, false);
        if (cart == null || !cart.Items.Any())
            return Result<PreviewCheckoutDto>.Failure("Cart is empty.");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Result<PreviewCheckoutDto>.Failure("User not found.");

        // 1. Chạy Promotion Engine
        var activePromotionRules = await _promotionRuleRepository.GetActivePromotionRulesAsync(DateTime.UtcNow);
        var engine = new PromotionEngine();
        var promotionResult = engine.ApplyBestRule(cart.Items, activePromotionRules);

        var variantIdsToFetch = cart.Items.Select(i => i.ProductVariantId)
            .Union(promotionResult.Gifts.Select(g => g.ProductVariantId))
            .Distinct().ToList();

        var variantsFromDb = await _productVariantRepository.GetListByIdsAsync(variantIdsToFetch);
        var variantDict = variantsFromDb.ToDictionary(v => v.Id);

        decimal subTotal = 0;
        var inventoryNeeded = new Dictionary<int, int>();
        var previewItems = new List<PreviewItemDto>();

        // Tính SubTotal, gom Tồn kho và TẠO DANH SÁCH PREVIEW
        foreach (var item in cart.Items)
        {
            if (!variantDict.TryGetValue(item.ProductVariantId, out var productVariant))
                return Result<PreviewCheckoutDto>.Failure($"Product variant {item.ProductVariantId} not found.");

            subTotal += productVariant.Price * item.Quantity;

            previewItems.Add(new PreviewItemDto
            {
                ProductVariantId = item.ProductVariantId,
                ProductName = productVariant.SKU,
                UnitPrice = productVariant.Price,
                Quantity = item.Quantity,
                IsGift = false
            });

            if (inventoryNeeded.ContainsKey(item.ProductVariantId))
                inventoryNeeded[item.ProductVariantId] += item.Quantity;
            else
                inventoryNeeded.Add(item.ProductVariantId, item.Quantity);
        }

        // Bổ sung QUÀ TẶNG vào danh sách Preview và gom tồn kho cần trừ cho quà tặng
        foreach (var gift in promotionResult.Gifts)
        {
            if (variantDict.TryGetValue(gift.ProductVariantId, out var giftVariant))
            {
                previewItems.Add(new PreviewItemDto
                {
                    ProductVariantId = gift.ProductVariantId,
                    ProductName = giftVariant.SKU,
                    UnitPrice = 0,
                    Quantity = gift.Quantity,
                    IsGift = true
                });

                if (inventoryNeeded.ContainsKey(gift.ProductVariantId))
                    inventoryNeeded[gift.ProductVariantId] += gift.Quantity;
                else
                    inventoryNeeded.Add(gift.ProductVariantId, gift.Quantity);
            }
        }

        //Cảnh báo sớm nếu hết hàng
        foreach (var kvp in inventoryNeeded)
        {
            var variantId = kvp.Key;
            var needed = kvp.Value;

            if (variantDict.TryGetValue(variantId, out var productVariant))
            {
                if (productVariant.StockQuantity < needed)
                    return Result<PreviewCheckoutDto>.Failure($"Sản phẩm (ID: {productVariant.Id}) không đủ tồn kho. Cần: {needed}, Còn: {productVariant.StockQuantity}");
            }
        }

        // Tính toán giá trị tổng
        decimal totalAfterPromotion = subTotal - promotionResult.TotalDiscount;
        if (totalAfterPromotion < 0) totalAfterPromotion = 0;

        decimal rankDiscountRate = RankService.GetDiscountRate(user.MemberRank);
        decimal rankDiscountAmount = totalAfterPromotion * rankDiscountRate;

        decimal totalAfterRank = totalAfterPromotion - rankDiscountAmount;
        if (totalAfterRank < 0) totalAfterRank = 0;

        decimal couponDiscountAmount = 0;

        //Tính Coupon nếu có nhập mã
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var appliedCoupon = await _couponRepository.GetValidCouponByCodeAsync(request.CouponCode, DateTime.UtcNow);
            if (appliedCoupon == null || appliedCoupon.UsageLimit <= 0)
                return Result<PreviewCheckoutDto>.Failure("Mã giảm giá không hợp lệ hoặc đã hết lượt sử dụng.");

            if (totalAfterRank < appliedCoupon.MinOrderValue)
                return Result<PreviewCheckoutDto>.Failure($"Đơn hàng cần đạt tối thiểu {appliedCoupon.MinOrderValue} để áp dụng mã này.");

            couponDiscountAmount = appliedCoupon.DiscountType == "PERCENTAGE"
                ? totalAfterRank * (appliedCoupon.Value / 100)
                : appliedCoupon.Value;
        }

        decimal finalTotal = totalAfterRank - couponDiscountAmount;
        if (finalTotal < 0) finalTotal = 0;

        return Result<PreviewCheckoutDto>.Success(new PreviewCheckoutDto
        {
            SubTotal = Math.Round(subTotal, 2, MidpointRounding.AwayFromZero),
            PromotionDiscount = Math.Round(promotionResult.TotalDiscount, 2, MidpointRounding.AwayFromZero),
            RankDiscount = Math.Round(rankDiscountAmount, 2, MidpointRounding.AwayFromZero),
            CouponDiscount = Math.Round(couponDiscountAmount, 2, MidpointRounding.AwayFromZero),
            FinalTotal = Math.Round(finalTotal, 2, MidpointRounding.AwayFromZero),
            Items = previewItems
        });
    }
}