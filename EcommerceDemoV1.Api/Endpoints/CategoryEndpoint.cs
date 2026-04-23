using EcommerceDemoV1.Application.Features.Category.Commands.CreateCategory;
using EcommerceDemoV1.Application.Features.Category.Queries.GetAllCategories;

using MediatR;

namespace EcommerceDemoV1.Api.Endpoints;

public static class CategoryEndpoint
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/categories").WithTags("Categories");

        group.MapPost("/", async (CreateCategoryCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(new { success = true, Category = result, message = "Category created successfully" });
        });

        group.MapGet("/", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAllCategoriesQuery());
            return Results.Ok(new { success = true, Categories = result, message = "Categories retrieved successfully" });
        });
    }
}