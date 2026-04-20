using MediatR;
using FluentValidation;
using EcommerceDemoV1.Application.Features.Reviews.Commands.CreateReview;

namespace EcommerceDemoV1.Api.Endpoints;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/reviews")
            .WithTags("User - Reviews");

        group.MapPost("/", async (
            CreateReviewCommand command,
            IMediator mediator,
            IValidator<CreateReviewCommand> validator) =>
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
            return Results.Ok(new { success = true, result, message = "Review created successfully" });
        }).RequireAuthorization();
    }
}