using MediatR;
using EcommerceDemoV1.Application.DTOs.Product;
using EcommerceDemoV1.Application.Common;

namespace EcommerceDemoV1.Application.Features.Product.Queries.GetProduct;

public record GetProductAdminQuery(
    int Page,
    int Size,
    string? Category = null
) : IRequest<PagedResult<ProductDto>>;

