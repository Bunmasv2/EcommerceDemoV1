using MediatR;
using EcommerceDemoV1.Application.DTOs.Product;

namespace EcommerceDemoV1.Application.Features.Product.Queries.GetProductById;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto>;