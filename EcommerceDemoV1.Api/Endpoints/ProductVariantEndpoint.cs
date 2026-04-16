using MediatR;
using Microsoft.AspNetCore.Mvc;
using EcommerceDemoV1.Application.Features.ProductVariant.Commands.UpdateProductVariant;
using EcommerceDemoV1.Application.Features.ProductVariant.Commands.DeleteProductVariant;
using EcommerceDemoV1.Application.Features.ProductVariant.Commands.CreateProductVariant;
using EcommerceDemoV1.Application.Features.ProductVariant.Queries.GetProductVariant;
using EcommerceDemoV1.Application.Features.ProductVariant.Queries.GetProductVariantById;



namespace EcommerceDemoV1.WebAPI.Endpoints;

public static class ProductVariantEndpoints
{
    public static void MapProductVariantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/variants").WithTags("Variants");

        group.MapPost("/", async (
            [FromBody] CreateProductVariantCommand command,
            IMediator mediator) =>
        {
            var variant = await mediator.Send(command);
            return Results.Ok(new { success = true, variant, message = "Product variant created successfully" });
        })
        .RequireAuthorization("AdminOnly");

        group.MapPut("/{id:int}", async (
            int id,
            [FromBody] UpdateProductVariantCommand bodyCommand,
            IMediator mediator) =>
        {
            var command = bodyCommand with { Id = id };

            var result = await mediator.Send(command);
            if (!result.IsSuccess)
            {
                return Results.NotFound(new { success = false, message = result.ErrorMessage });
            }
            return Results.Ok(new { success = true, result, message = "Product variant updated successfully" });
        })
        .RequireAuthorization("AdminOnly");

        group.MapDelete("/{id:int}", async (
            int id,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteProductVariantCommand(id));
            if (!result.IsSuccess)
            {
                return Results.NotFound(new { success = false, message = result.ErrorMessage });
            }
            return Results.Ok(new { success = true, result, message = "Product variant deleted successfully" });
        })
        .RequireAuthorization("AdminOnly");

        group.MapGet("/", async (
            int? page,
            int? size,
            string? category,
            decimal? minPrice,
            decimal? maxPrice,
            IMediator mediator) =>
        {
            var query = new GetProductVariantQuery(page ?? 1, size ?? 10, category, minPrice, maxPrice);
            var products = await mediator.Send(query);
            return Results.Ok(products);
        });

        group.MapGet("/byAdmin", async (
            int? page,
            int? size,
            IMediator mediator) =>
        {
            var query = new GetProductVariantAdminQuery(page ?? 1, size ?? 10);
            var products = await mediator.Send(query);
            return Results.Ok(products);
        })
        .RequireAuthorization("AdminOnly");


        group.MapGet("/{id:int}", async (
            int id,
            IMediator mediator) =>
        {
            try
            {
                var product = await mediator.Send(new GetProductVariantByIdQuery(id));
                return Results.Ok(product);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { message = "Product variant not found" });
            }
        });
    }
}