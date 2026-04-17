using MediatR;
using FluentValidation;
using EcommerceDemoV1.Application.Features.Orders.Commands;
using EcommerceDemoV1.Application.Features.Orders.Commands.Checkout;

namespace EcommerceDemoV1.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orders")
            .WithTags("User - Orders")
            .RequireAuthorization();

        group.MapPost("/checkout", async (
            CheckoutCommand command,
            IMediator mediator,
            IValidator<CheckoutCommand> validator) =>
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
    }
}
