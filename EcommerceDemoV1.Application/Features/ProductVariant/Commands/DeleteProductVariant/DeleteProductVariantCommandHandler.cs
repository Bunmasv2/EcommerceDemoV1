using MediatR;
using EcommerceDemoV1.Application.DTOs.Product;
using EcommerceDemoV1.Domain.Entities;

namespace EcommerceDemoV1.Application.Features.ProductVariant.Commands.DeleteProductVariant;

public record DeleteProductVariantCommandHandler : IRequestHandler<DeleteProductVariantCommand, Result<bool>>
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductVariantCommandHandler(IProductVariantRepository productVariantRepository, IUnitOfWork unitOfWork)
    {
        _productVariantRepository = productVariantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteProductVariantCommand request, CancellationToken cancellationToken)
    {
        var existingVariant = await _productVariantRepository.ExistsAsync(request.Id);
        if (!existingVariant)
        {
            return Result<bool>.Failure($"Product variant with ID {request.Id} not found.");
        }

        await _productVariantRepository.DeleteAsync(request.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}