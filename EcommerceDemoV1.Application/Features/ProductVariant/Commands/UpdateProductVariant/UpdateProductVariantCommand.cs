using MediatR;
using EcommerceDemoV1.Application.DTOs.Product;
namespace EcommerceDemoV1.Application.Features.ProductVariant.Commands.UpdateProductVariant;

public record UpdateProductVariantCommand(
    int Id,
    int ProductId,
    string? SKU,
    string? Color,
    string? Size,
    decimal? Price,
    int? StockQuantity
) : IRequest<Result<ProductVariantDto>>;