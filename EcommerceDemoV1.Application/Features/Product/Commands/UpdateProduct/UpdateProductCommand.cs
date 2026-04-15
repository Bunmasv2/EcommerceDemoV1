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
) : IRequest<Unit>; // Dùng Unit thay cho void trong MediatR