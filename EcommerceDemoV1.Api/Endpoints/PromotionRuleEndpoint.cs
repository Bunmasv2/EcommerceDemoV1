using MediatR;
using FluentValidation;
using EcommerceDemoV1.Application.Features.PromotionRules.Commands.CreatePromotionRule;

namespace EcommerceDemoV1.Api.Endpoints;

public static class PromotionRuleEndpoint
{
    public static void MapPromotionRuleEndpoints(this IEndpointRouteBuilder app)
    {
        var adminGroup = app.MapGroup("/api/v1/admin/promotions")
            .WithTags("Admin - Promotions")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        adminGroup.MapPost("/", async (
            CreatePromotionRuleCommand command,
            IMediator mediator,
            IValidator<CreatePromotionRuleCommand> validator) =>
        {
            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.ToDictionary());

            var result = await mediator.Send(command);
            if (!result.IsSuccess)
                return Results.BadRequest(new { success = false, message = result.ErrorMessage });

            return Results.Ok(new { success = true, result, message = "Promotion rule created successfully" });
        }).RequireAuthorization();
    }
}