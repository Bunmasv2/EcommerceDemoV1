using MediatR;
using Microsoft.AspNetCore.Mvc;
using EcommerceDemoV1.Application.Features.Product.Commands.CreateProduct;
using EcommerceDemoV1.Application.Features.Product.Commands.UpdateProduct;
using EcommerceDemoV1.Application.Features.Product.Commands.DeleteProduct;
using EcommerceDemoV1.Application.Features.Product.Queries.GetProductById;
using EcommerceDemoV1.Application.Features.Product.Queries.GetProduct;

namespace EcommerceDemoV1.WebAPI.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/products").WithTags("Products");

        group.MapPost("/", async (
            [FromBody] CreateProductCommand command,
            IMediator mediator) =>
        {
            var productId = await mediator.Send(command);
            return Results.Ok(new { success = true, productId, message = "Product created successfully" });
        })
        .RequireAuthorization("AdminOnly");

        group.MapPut("/{id:int}", async (
            int id,
            [FromBody] UpdateProductCommand bodyCommand,
            IMediator mediator) =>
        {
            var command = bodyCommand with { Id = id };

            await mediator.Send(command);
            return Results.Ok(new { success = true, message = "Product updated successfully" });
        })
        .RequireAuthorization("AdminOnly");

        group.MapDelete("/{id:int}", async (
            int id,
            IMediator mediator) =>
        {
            await mediator.Send(new DeleteProductCommand(id));
            return Results.Ok(new { success = true, message = "Product deleted successfully" });
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
            var query = new GetProductsQuery(page ?? 1, size ?? 10, category, minPrice, maxPrice);
            var products = await mediator.Send(query);
            return Results.Ok(products);
        });

        group.MapGet("/{id:int}", async (
            int id,
            IMediator mediator) =>
        {
            try
            {
                var product = await mediator.Send(new GetProductByIdQuery(id));
                return Results.Ok(product);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { message = "Product not found" });
            }
        });
    }
}