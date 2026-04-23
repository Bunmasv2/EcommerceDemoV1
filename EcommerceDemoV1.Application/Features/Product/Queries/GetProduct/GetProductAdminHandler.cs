using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using EcommerceDemoV1.Application.DTOs.Product;
using EcommerceDemoV1.Application.Common;

namespace EcommerceDemoV1.Application.Features.Product.Queries.GetProduct;

public class GetProductAdminQueryHandler : IRequestHandler<GetProductAdminQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductAdminQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductAdminQuery request, CancellationToken cancellationToken)
    {
        var items = await _productRepository.GetAllAsyncByAdmin(
            request.Page,
            request.Size);
        var totalCount = items.Count();

        var productDtos = items.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            CategoryName = p.Category.Name ?? "",
            BasePrice = p.BasePrice,
            Description = p.Description,
            ImageUrl = p.ImageUrl,
            Variants = p.Variants.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                SKU = v.SKU,
                Color = v.Color,
                Size = v.Size,
                Price = v.Price,
                StockQuantity = v.StockQuantity
            }).ToList()
        }).ToList();

        return new PagedResult<ProductDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        };
    }
}