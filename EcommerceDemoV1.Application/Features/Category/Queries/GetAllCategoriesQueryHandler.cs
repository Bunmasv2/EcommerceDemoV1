using MediatR;

namespace EcommerceDemoV1.Application.Features.Category.Queries.GetAllCategories;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, Result<List<CategoryDtoResponse>>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<List<CategoryDtoResponse>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllCategoriesAsync();
        return Result<List<CategoryDtoResponse>>.Success(categories.Select(c => new CategoryDtoResponse
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        }).ToList());
    }
}