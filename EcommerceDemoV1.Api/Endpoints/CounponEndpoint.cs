using MediatR;
using FluentValidation;
using EcommerceDemoV1.Application.Features.Coupons.Commands.CreateCoupon;
using EcommerceDemoV1.Application.Features.Coupons.Queries.GetCoupon;

namespace EcommerceDemoV1.Api.Endpoints;

public static class CouponEndpoint
{
    public static void MapCouponEndpoints(this IEndpointRouteBuilder app)
    {
        var adminGroup = app.MapGroup("/api/v1/admin/coupons")
            .WithTags("Admin - Coupons")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        adminGroup.MapPost("/", async (
            CreateCouponCommand command,
            IMediator mediator,
            IValidator<CreateCouponCommand> validator) =>
        {

            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.ToDictionary());
            }

            var result = await mediator.Send(command);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { success = false, message = result.ErrorMessage });
            }
            return Results.Ok(new { success = true, result, message = "Order created successfully" });
        }).RequireAuthorization();

        var userGroup = app.MapGroup("/api/v1/coupons")
            .WithTags("Coupons")
            .RequireAuthorization();

        userGroup.MapGet("/", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCouponCommand());
            return Results.Ok(new { success = true, Coupons = result, message = "Coupons retrieved successfully" });
        }).RequireAuthorization();
    }
}