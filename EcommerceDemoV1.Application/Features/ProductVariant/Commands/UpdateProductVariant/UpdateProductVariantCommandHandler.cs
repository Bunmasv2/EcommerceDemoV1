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
        var existingVariant = await _productVariantRepository.ExistsAsync(request.Id);
        if (!existingVariant)
        {
            return Result<ProductVariantDto>.Failure($"Product variant with ID {request.Id} not found.");
        }

        var variantUpdate = new EcommerceDemoV1.Domain.Entities.ProductVariant
        {
            Id = request.Id,
            ProductId = request.ProductId,
            Color = request.Color,
            Size = request.Size,
            Price = request.Price,
            StockQuantity = request.StockStockQuantity
        };

        await _productVariantRepository.UpdateAsync(variantUpdate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var variantDto = new ProductVariantDto
        {
            Id = variantUpdate.Id,
            Color = variantUpdate.Color,
            Size = variantUpdate.Size,
            Price = variantUpdate.Price,
            StockQuantity = variantUpdate.StockQuantity
        };

        return Result<ProductVariantDto>.Success(variantDto);
    }
}