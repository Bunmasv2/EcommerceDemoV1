using MediatR;
using EcommerceDemoV1.Application.DTOs.Product;

namespace EcommerceDemoV1.Application.Features.Product.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    int CategoryId,
    decimal BasePrice,
    string? Description,
    string? ImageUrl
) : IRequest<ProductDtoRespone>;