using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EcommerceDemoV1.Application.DTOs.Product;



namespace EcommerceDemoV1.Application.Features.Product.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id);

        if (product == null)
            throw new KeyNotFoundException("Product not found");

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            CategoryName = product.Category?.Name ?? "",
            BasePrice = product.BasePrice,
        };
    }
}