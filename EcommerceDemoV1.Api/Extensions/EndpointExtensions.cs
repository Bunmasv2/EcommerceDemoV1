using EcommerceDemoV1.Api.Endpoints;

namespace EcommerceDemoV1.Api.Extensions;

public static class EndpointExtensions
{
    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAuthEndpoints();
        app.MapProductEndpoints();
        app.MapProductVariantEndpoints();
        app.MapCartEndpoints();
        app.MapCouponEndpoints();
        app.MapPromotionRuleEndpoints();
        app.MapOrderEndpoints();
        app.MapPaymentEndpoints();
        app.MapReviewEndpoints();
    }
}