using MediatR;
using EcommerceDemoV1.Application.DTOs.Product;

public record UpdateProductCommand(
    int Id,
    string Name,
    int CategoryId,
    decimal BasePrice,
    string? Description,
    string? ImageUrl
// List<ProductVariantDto> Variants
) : IRequest<Result<ProductDtoRespone>>;