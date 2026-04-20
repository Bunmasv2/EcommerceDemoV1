using MediatR;
using FluentValidation;
using EcommerceDemoV1.Application.Features.Payment.Commands.CreatePayOsLink;
using PayOS.Models;
using System.Text.Json;
using EcommerceDemoV1.Application.Features.Payment.Commands.ProcessWebhook;

namespace EcommerceDemoV1.Api.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var groupPayments = app.MapGroup("/api/v1/payments")
            .WithTags("User - Payments");

        // TASK 10.1: POST /api/v1/payments/create
        groupPayments.MapPost("/create", async (CreatePayOsLinkCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { success = false, message = result.ErrorMessage });
            }
            return Results.Ok(new { success = true, paymentUrl = result, message = "Payment link created successfully" });
        }).RequireAuthorization();

        groupPayments.MapPost("/webhook/payos", async (JsonElement webhookBody, IMediator mediator) =>
        {
            var command = new ProcessPayOsWebhookCommand(webhookBody);
            var result = await mediator.Send(command);

            // Theo tài liệu của PayOS, khi nhận được Webhook, phải trả về JSON chuẩn này để họ biết mình đã nhận
            return Results.Ok(new { success = true });
        });
    }
}
