using EcommerceDemoV1.WebAPI.Endpoints;

namespace EcommerceDemoV1.Api.Extensions;

public static class EndpointExtensions
{
    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAuthEndpoints();
        app.MapProductEndpoints();
        app.MapProductVariantEndpoints();
        // app.MapOrderEndpoints();
    }
}