using MediatR;
using FluentValidation;
using EcommerceDemoV1.Application.Features.Orders.Commands;
using EcommerceDemoV1.Application.Features.Orders.Commands.Checkout;
using EcommerceDemoV1.Application.Features.Orders.Queries.GetOrders;
using EcommerceDemoV1.Application.Features.Orders.Commands.UpdateOrder;
using EcommerceDemoV1.Application.Features.Orders.Queries.PreviewCheckout;

namespace EcommerceDemoV1.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orders")
            .WithTags("User - Orders")
            .RequireAuthorization();

        group.MapGet("/", async (
            int? page,
            int? size,
            string? search,
            IMediator mediator,
            IValidator<GetOrderCommand> validator) =>
        {
            var command = new GetOrderCommand(page ?? 1, size ?? 10, search);
            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.ToDictionary());
            }

            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        // TASK 4.1: POST /api/v1/orders/checkout
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

        // TASK 4.2: PUT /api/v1/orders/{orderId}/status
        group.MapPut("/{orderId}/status", async (
            int orderId,
            UpdateStatusOrderCommand command,
            IMediator mediator,
            IValidator<UpdateStatusOrderCommand> validator) =>
        {
            if (orderId != command.orderId)
            {
                return Results.BadRequest(new { success = false, message = "Order ID in URL does not match Order ID in body" });
            }

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
            return Results.Ok(new { success = true, result, message = "Order status updated successfully" });
        }).RequireAuthorization();

        group.MapPost("/preview-checkout", async (
            PreviewCheckoutQuery query,
            IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.ErrorMessage);
        }).RequireAuthorization();
    }
}
