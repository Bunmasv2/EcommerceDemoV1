using MediatR;

namespace EcommerceDemoV1.Application.Features.Category.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDtoResponse>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CategoryDtoResponse>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new EcommerceDemoV1.Domain.Entities.Category
        {
            Name = request.Name,
            Description = request.Description
        };

        var createdCategory = await _categoryRepository.CreateCategoryAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CategoryDtoResponse>.Success(new CategoryDtoResponse
        {
            Id = createdCategory.Id,
            Name = createdCategory.Name,
            Description = createdCategory.Description
        });
    }
}