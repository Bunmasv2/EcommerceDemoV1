using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using EcommerceDemoV1.Application.DTOs.Product;
using EcommerceDemoV1.Application.Common;

namespace EcommerceDemoV1.Application.Features.ProductVariant.Queries.GetProductVariant;

public class GetProductVariantAdminQueryHandler : IRequestHandler<GetProductVariantAdminQuery, PagedResult<ProductVariantDto>>
{
    private readonly IProductVariantRepository _productVariantRepository;

    public GetProductVariantAdminQueryHandler(IProductVariantRepository productVariantRepository)
    {
        _productVariantRepository = productVariantRepository;
    }

    public async Task<PagedResult<ProductVariantDto>> Handle(GetProductVariantAdminQuery request, CancellationToken cancellationToken)
    {
        var items = await _productVariantRepository.GetAllAsyncByAdmin(request.Page, request.Size);
        var totalCount = items.Count();

        var productDtos = items.Select(p => new ProductVariantDto
        {
            Id = p.Id,
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