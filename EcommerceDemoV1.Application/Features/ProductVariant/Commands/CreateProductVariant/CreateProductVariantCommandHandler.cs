using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using EcommerceDemoV1.Application.DTOs.Product;

namespace EcommerceDemoV1.Application.Features.ProductVariant.Commands.CreateProductVariant;

public class CreateProductVariantCommandHandler : IRequestHandler<CreateProductVariantCommand, List<ProductVariantDto>>
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductVariantCommandHandler(IProductVariantRepository productVariantRepository, IUnitOfWork unitOfWork)
    {
        _productVariantRepository = productVariantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProductVariantDto>> Handle(CreateProductVariantCommand request, CancellationToken cancellationToken)
    {

        var productVariants = request.Variants.Select(v => new EcommerceDemoV1.Domain.Entities.ProductVariant
        {
            ProductId = v.ProductId,
            SKU = v.SKU,
            Color = v.Color,
            Size = v.Size,
            Price = v.Price,
            StockQuantity = v.StockQuantity
        }).ToList();

        var createdVariants = await _productVariantRepository.CreateRangeAsync(productVariants);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return createdVariants.Select(v => new ProductVariantDto
        {
            Id = v.Id,
            ProductId = v.ProductId,
            SKU = v.SKU,
            Color = v.Color,
            Size = v.Size,
            Price = v.Price,
            StockQuantity = v.StockQuantity
        }).ToList();
    }
}