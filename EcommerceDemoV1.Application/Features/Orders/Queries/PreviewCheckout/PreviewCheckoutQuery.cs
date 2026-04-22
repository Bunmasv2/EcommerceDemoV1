using MediatR;

namespace EcommerceDemoV1.Application.Features.Orders.Queries.PreviewCheckout;

public record PreviewCheckoutQuery(string? CouponCode) : IRequest<Result<PreviewCheckoutDto>>;