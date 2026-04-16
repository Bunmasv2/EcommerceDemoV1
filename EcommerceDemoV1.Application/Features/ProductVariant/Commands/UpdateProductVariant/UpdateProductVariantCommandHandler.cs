using MediatR;
using EcommerceDemoV1.Application.DTOs.Product;

namespace EcommerceDemoV1.Application.Features.ProductVariant.Commands.UpdateProductVariant;

public class UpdateProductVariantCommandHandler : IRequestHandler<UpdateProductVariantCommand, Result<ProductVariantDto>>
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductVariantCommandHandler(IProductVariantRepository productVariantRepository, IUnitOfWork unitOfWork)
    {
        _productVariantRepository = productVariantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductVariantDto>> Handle(UpdateProductVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await _productVariantRepository.GetByIdAsync(request.Id);

        if (variant == null)
        {
            return Result<ProductVariantDto>.Failure($"Product variant with ID {request.Id} not found.");
        }

        variant.SKU = request.SKU ?? variant.SKU;
        variant.Color = request.Color ?? variant.Color;
        variant.Size = request.Size ?? variant.Size;
        variant.Price = request.Price ?? variant.Price;
        variant.StockQuantity = request.StockQuantity ?? variant.StockQuantity;

        if (request.ProductId != 0) variant.ProductId = request.ProductId;

        await _productVariantRepository.UpdateAsync(variant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var variantDto = new ProductVariantDto
        {
            Id = variant.Id,
            ProductId = variant.ProductId,
            SKU = variant.SKU,
            Color = variant.Color,
            Size = variant.Size,
            Price = variant.Price,
            StockQuantity = variant.StockQuantity
        };

        return Result<ProductVariantDto>.Success(variantDto);
    }
}