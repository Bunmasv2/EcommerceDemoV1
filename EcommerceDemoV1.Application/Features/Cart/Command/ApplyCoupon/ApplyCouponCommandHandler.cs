using MediatR;
using EcommerceDemoV1.Application.Common;
using EcommerceDemoV1.Domain.Entities;

namespace EcommerceDemoV1.Application.Features.Cart.Commands.ApplyCoupon;

public class AppllyCouponCommandHandler : IRequestHandler<ApplyCouponCommand, Result<CouponCalculationDto>>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AppllyCouponCommandHandler(ICartRepository cartRepository, ICouponRepository couponRepository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _couponRepository = couponRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CouponCalculationDto>> Handle(ApplyCouponCommand request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(_currentUserService.UserId!);

        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart == null || !cart.Items.Any())
            return Result<CouponCalculationDto>.Failure("cart is empty, can't apply coupon.");

        decimal currentTotal = cart.Items.Sum(item => item.ProductVariant.Price * item.Quantity);

        var coupon = await _couponRepository.GetByCodeAsync(request.CouponCode.ToUpper());
        if (coupon == null)
            return Result<CouponCalculationDto>.Failure("Invalid coupon code or coupon is blocked.");

        var now = DateTime.UtcNow;
        if (coupon.StartDate > now || coupon.EndDate < now)
            return Result<CouponCalculationDto>.Failure("Coupon is expired.");
        if (coupon.UsedCount >= coupon.UsageLimit)
            return Result<CouponCalculationDto>.Failure("Coupon usage limit reached.");
        if (coupon.UsageLimit <= 0)
            return Result<CouponCalculationDto>.Failure("Coupon usage limit reached.");
        if (currentTotal < coupon.MinOrderValue)
            return Result<CouponCalculationDto>.Failure("Order value does not meet the minimum requirement for this coupon.");

        decimal discountAmount = 0;
        if (coupon.DiscountType == "PERCENTAGE")
        {
            discountAmount = currentTotal * (coupon.Value / 100);
        }
        else if (coupon.DiscountType == "FIXED_AMOUNT")
        {
            discountAmount = coupon.Value;
        }

        discountAmount = Math.Min(discountAmount, currentTotal); // Discount can't exceed current total

        cart.AppliedCouponCode = coupon.Code;
        cart.AppliedDiscount = discountAmount;
        await _cartRepository.UpdateCartAsync(cart);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CouponCalculationDto>.Success(new CouponCalculationDto(
            coupon.Code,
            discountAmount,
            currentTotal - discountAmount)
        );

    }
}