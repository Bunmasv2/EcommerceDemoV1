using MediatR;
using EcommerceDemoV1.Application.DTOs.Product;

namespace EcommerceDemoV1.Application.Features.ProductVariant.Queries.GetProductVariantById;

public record GetProductVariantByIdQuery(int Id) : IRequest<ProductVariantDto>;