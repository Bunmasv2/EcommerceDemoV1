using MediatR;
using EcommerceDemoV1.Domain.Entities;

namespace EcommerceDemoV1.Application.Features.Coupons.Queries.GetCoupon;

public class GetCouponCommandHandler : IRequestHandler<GetCouponCommand, Result<List<Coupon>>>
{
    private readonly ICouponRepository _couponRepository;

    public GetCouponCommandHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<Result<List<Coupon>>> Handle(GetCouponCommand request, CancellationToken cancellationToken)
    {
        var coupons = await _couponRepository.GetAllAsync();
        return Result<List<Coupon>>.Success(coupons);
    }
}