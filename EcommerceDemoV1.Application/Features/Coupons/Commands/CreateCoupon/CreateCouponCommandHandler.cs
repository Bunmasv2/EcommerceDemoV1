using MediatR;
using EcommerceDemoV1.Domain.Entities;
using EcommerceDemoV1.Application.Common;

namespace EcommerceDemoV1.Application.Features.Coupons.Commands.CreateCoupon;

public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, Result<int>>
{
    private readonly ICouponRepository _couponRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCouponCommandHandler(ICouponRepository couponRepository, IUnitOfWork unitOfWork)
    {
        _couponRepository = couponRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var existingCoupon = await _couponRepository.IsExistingCodeAsync(request.Code);
        if (existingCoupon)
            return Result<int>.Failure("Coupon code already exists.");

        var coupon = new Coupon
        {
            Code = request.Code,
            DiscountType = request.DiscountType,
            Value = request.Value,
            MinOrderValue = request.MinOrderValue,
            UsageLimit = request.UsageLimit,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
        await _couponRepository.AddCouponAsync(coupon);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(coupon.Id);
    }
}