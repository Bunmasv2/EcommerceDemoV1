using MediatR;

namespace EcommerceDemoV1.Application.Features.Category.Queries.GetAllCategories;

public record GetAllCategoriesQuery : IRequest<Result<List<CategoryDtoResponse>>>;