using MediatR;
using EcommerceDemoV1.Application.Features.Cart.Command.AddToCart;
using EcommerceDemoV1.Application.Features.Cart.Queries.GetCart;
using EcommerceDemoV1.Application.Features.Cart.Commands.ApplyCoupon;

using FluentValidation;

namespace EcommerceDemoV1.Api.Endpoints;

public static class CartEndpoint
{
    public static void MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/carts").WithTags("Carts");

        group.MapPost("/", async (
            AddToCartCommand command,
            IMediator mediator,
            IValidator<AddToCartCommand> validator) =>
        {
            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.ToDictionary());
            }

            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization("UserOnly");

        group.MapGet("/", async (
            IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCartQuery());
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/apply-coupon", async (
            ApplyCouponCommand command,
            IMediator mediator,
            IValidator<ApplyCouponCommand> validator) =>
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
            return Results.Ok(new { success = true, result, message = "Coupon applied successfully" });
        }).RequireAuthorization();
    }
}