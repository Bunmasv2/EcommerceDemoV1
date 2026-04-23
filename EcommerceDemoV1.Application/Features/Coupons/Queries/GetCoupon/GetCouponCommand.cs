using MediatR;
using EcommerceDemoV1.Domain.Entities;

namespace EcommerceDemoV1.Application.Features.Coupons.Queries.GetCoupon;

public record GetCouponCommand() : IRequest<Result<List<Coupon>>>;