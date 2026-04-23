using MediatR;
using EcommerceDemoV1.Application.Common;

namespace EcommerceDemoV1.Application.Features.Cart.Commands.ApplyCoupon;

public record ApplyCouponCommand(string couponCode) : IRequest<Result<CouponCalculationDto>>;