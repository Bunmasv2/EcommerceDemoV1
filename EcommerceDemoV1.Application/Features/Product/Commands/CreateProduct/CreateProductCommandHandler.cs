using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace EcommerceDemoV1.Application.Features.Product.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Map từ Command sang Entity
        var product = new EcommerceDemoV1.Domain.Entities.Product
        {
            Name = request.Name,
            CategoryId = request.CategoryId,
            BasePrice = request.BasePrice,
            Description = request.Description,
            ImageUrl = request.ImageUrl
        };

        var productId = await _productRepository.CreateAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return productId;
    }
}