using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EcommerceDemoV1.Application.DTOs.Product;



namespace EcommerceDemoV1.Application.Features.ProductVariant.Queries.GetProductVariantById;

public class GetProductVariantByIdQueryHandler : IRequestHandler<GetProductVariantByIdQuery, ProductVariantDto>
{
    private readonly IProductVariantRepository _productVariantRepository;

    public GetProductVariantByIdQueryHandler(IProductVariantRepository productVariantRepository)
    {
        _productVariantRepository = productVariantRepository;
    }

    public async Task<ProductVariantDto> Handle(GetProductVariantByIdQuery request, CancellationToken cancellationToken)
    {
        var productVariant = await _productVariantRepository.GetByIdAsync(request.Id);

        if (productVariant == null)
            throw new KeyNotFoundException("Product variant not found");

        return new ProductVariantDto
        {
            Id = productVariant.Id,
            SKU = productVariant.SKU,
            Color = productVariant.Color,
            Size = productVariant.Size,
            Price = productVariant.Price,
            StockQuantity = productVariant.StockQuantity,
        };
    }
}