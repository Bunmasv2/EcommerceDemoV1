using MediatR;
using EcommerceDemoV1.Application.DTOs.Product;
using EcommerceDemoV1.Application.Common;

namespace EcommerceDemoV1.Application.Features.Product.Queries.GetProduct;

public record GetProductsQuery(
    int Page,
    int Size,
    string? Category = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null
) : IRequest<PagedResult<ProductDto>>;
