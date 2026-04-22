using FluentValidation;
using MediatR;

namespace EcommerceDemoV1.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth");

        group.MapPost("/register", async (
            RegisterUserCommand command,
            IMediator mediator,
            IValidator<RegisterUserCommand> validator
            ) =>
        {
            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return Results.BadRequest(errors);
            }
            var userId = await mediator.Send(command);
            return Results.Ok(new { userId, email = command.Email });
        }).WithTags("Auth");

        group.MapPost("/login", async (
            LoginUserCommand command,
            IMediator mediator,
            IValidator<LoginUserCommand> validator,
            HttpContext context
            ) =>
        {
            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return Results.BadRequest(errors);
            }

            var authResponse = await mediator.Send(command);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(180)
            };

            context.Response.Cookies.Append("AccessToken", authResponse.AccessToken, cookieOptions);

            return Results.Ok(new { Message = "Login successful", User = authResponse.infoUser });
        }).WithTags("Auth");
    }
}