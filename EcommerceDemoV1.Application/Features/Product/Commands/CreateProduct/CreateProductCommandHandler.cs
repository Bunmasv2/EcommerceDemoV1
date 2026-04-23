using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using EcommerceDemoV1.Application.DTOs.Product;

namespace EcommerceDemoV1.Application.Features.Product.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDtoRespone>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDtoRespone> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new EcommerceDemoV1.Domain.Entities.Product
        {
            Name = request.Name,
            CategoryId = request.CategoryId,
            BasePrice = request.BasePrice,
            Description = request.Description,
            ImageUrl = request.ImageUrl
        };

        var Product = await _productRepository.CreateAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var ProductDto = new ProductDtoRespone
        {
            Id = Product.Id,
            Name = Product.Name,
            CategoryId = Product.CategoryId,
            BasePrice = Product.BasePrice,
            Description = Product.Description,
            ImageUrl = Product.ImageUrl
        };
        return ProductDto;
    }
}