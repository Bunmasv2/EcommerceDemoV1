using MediatR;

namespace EcommerceDemoV1.Application.Features.ProductVariant.Commands.DeleteProductVariant;

public record DeleteProductVariantCommand(
    int Id
) : IRequest<Result<bool>>;