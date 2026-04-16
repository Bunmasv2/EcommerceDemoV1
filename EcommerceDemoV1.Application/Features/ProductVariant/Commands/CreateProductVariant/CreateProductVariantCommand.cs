using MediatR;
using EcommerceDemoV1.Application.DTOs.Product;

namespace EcommerceDemoV1.Application.Features.ProductVariant.Commands.CreateProductVariant;

public record CreateProductVariantCommand(
    List<ProductVariantRequestDto> Variants
) : IRequest<List<ProductVariantDto>>;