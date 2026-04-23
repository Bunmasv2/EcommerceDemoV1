using MediatR;
using EcommerceDemoV1.Application.DTOs.Product;
using EcommerceDemoV1.Application.Common;

namespace EcommerceDemoV1.Application.Features.ProductVariant.Queries.GetProductVariant;

public record GetProductVariantAdminQuery(
    int Page,
    int Size,
    string? Category = null
) : IRequest<PagedResult<ProductVariantDto>>;

