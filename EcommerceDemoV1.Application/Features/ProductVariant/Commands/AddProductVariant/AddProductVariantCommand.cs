using MediatR;

namespace EcommerceDemoV1.Application.Features.ProductVariant.Commands.AddVariant;

public record AddProductVariantCommand(
    int ProductId,
    string SKU,
    string? Color,
    string? Size,
    decimal Price,
    int StockQuantity
) : IRequest<int>;