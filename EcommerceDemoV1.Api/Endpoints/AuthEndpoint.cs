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

            if (!authResponse.IsSuccess)
            {
                return Results.BadRequest(new { success = false, message = authResponse.ErrorMessage });
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            context.Response.Cookies.Append("AccessToken", authResponse.Data.AccessToken, cookieOptions);
            context.Response.Cookies.Append("RefreshToken", authResponse.Data.RefreshToken, refreshCookieOptions);

            return Results.Ok(new { Message = "Login successful", User = authResponse.Data.infoUser });
        }).WithTags("Auth");

        group.MapPost("/refresh", async (IMediator mediator, HttpContext context) =>
        {
            if (!context.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
            {
                return Results.Unauthorized();
            }

            try
            {
                var command = new RefreshTokenCommand(refreshToken);
                var authResponse = await mediator.Send(command);

                var accessOptions = new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Expires = DateTime.UtcNow.AddMinutes(15) };
                var refreshOptions = new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Expires = DateTime.UtcNow.AddDays(7) };

                context.Response.Cookies.Append("AccessToken", authResponse.AccessToken, accessOptions);
                context.Response.Cookies.Append("RefreshToken", authResponse.RefreshToken, refreshOptions);

                return Results.Ok(new { Message = "Token refreshed successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                context.Response.Cookies.Delete("AccessToken");
                context.Response.Cookies.Delete("RefreshToken");
                return Results.Unauthorized();
            }
        }).WithTags("Auth");
    }
}