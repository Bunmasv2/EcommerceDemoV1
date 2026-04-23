using MediatR;

namespace EcommerceDemoV1.Application.Features.Category.Commands.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    string? Description
) : IRequest<Result<CategoryDtoResponse>>;