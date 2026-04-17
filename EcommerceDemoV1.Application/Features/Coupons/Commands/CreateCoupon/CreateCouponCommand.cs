using MediatR;
using EcommerceDemoV1.Application.Common;

namespace EcommerceDemoV1.Application.Features.Coupons.Commands.CreateCoupon;

public record CreateCouponCommand(
    string Code,
    string DiscountType,
    decimal Value,
    decimal MinOrderValue,
    int UsageLimit,
    DateTime StartDate,
    DateTime EndDate
) : IRequest<Result<int>>;