using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using EcommerceDemoV1.Application.DTOs.Product;
using EcommerceDemoV1.Application.Common;

namespace EcommerceDemoV1.Application.Features.ProductVariant.Queries.GetProductVariant;

public class GetProductVariantQueryHandler : IRequestHandler<GetProductVariantQuery, PagedResult<ProductVariantDto>>
{
    private readonly IProductVariantRepository _productVariantRepository;

    public GetProductVariantQueryHandler(IProductVariantRepository productVariantRepository)
    {
        _productVariantRepository = productVariantRepository;
    }

    public async Task<PagedResult<ProductVariantDto>> Handle(GetProductVariantQuery request, CancellationToken cancellationToken)
    {
        var items = await _productVariantRepository.GetPagedAsync(
            request.Page,
            request.Size,
            request.Category,
            request.MinPrice,
            request.MaxPrice);
        var totalCount = items.TotalCount;

        var productDtos = items.Items.Select(p => new ProductVariantDto
        {
            Id = p.Id,
            ProductId = p.ProductId,
            SKU = p.SKU,
            Color = p.Color,
            Size = p.Size,
            Price = p.Price,
            StockQuantity = p.StockQuantity
        }).ToList();

        return new PagedResult<ProductVariantDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        };
    }
}