using MediatR;
using EcommerceDemoV1.Application.DTOs.Product;


namespace EcommerceDemoV1.Application.Features.Product.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDtoRespone>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductDtoRespone>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var productUpdate = new EcommerceDemoV1.Domain.Entities.Product
        {
            Id = request.Id,
            Name = request.Name,
            CategoryId = request.CategoryId,
            BasePrice = request.BasePrice,
            Description = request.Description,
            ImageUrl = request.ImageUrl
        };

        var product = await _productRepository.ExistsAsync(request.Id);
        if (!product)
        {
            return Result<ProductDtoRespone>.Failure($"Không tìm thấy product với ID {request.Id}");
        }

        await _productRepository.UpdateAsync(productUpdate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ProductDtoRespone>.Success(new ProductDtoRespone
        {
            Id = productUpdate.Id,
            Name = productUpdate.Name,
            CategoryId = productUpdate.CategoryId,
            BasePrice = productUpdate.BasePrice,
            Description = productUpdate.Description,
            ImageUrl = productUpdate.ImageUrl
        });
    }
}