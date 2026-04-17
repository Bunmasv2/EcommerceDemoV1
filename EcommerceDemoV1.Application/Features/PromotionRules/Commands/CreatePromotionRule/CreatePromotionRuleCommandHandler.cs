using EcommerceDemoV1.Application.Common;
using MediatR;
using EcommerceDemoV1.Domain.Entities;

namespace EcommerceDemoV1.Application.Features.PromotionRules.Commands.CreatePromotionRule;

public class CreatePromotionRuleHandler : IRequestHandler<CreatePromotionRuleCommand, Result<int>>
{
    private readonly IPromotionRuleRepository _promotionRuleRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IUnitOfWork _unitOfWork;


    public CreatePromotionRuleHandler(IPromotionRuleRepository promotionRuleRepository, ICategoryRepository categoryRepository, IProductVariantRepository productVariantRepository, IUnitOfWork unitOfWork)
    {
        _promotionRuleRepository = promotionRuleRepository;
        _categoryRepository = categoryRepository;
        _productVariantRepository = productVariantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreatePromotionRuleCommand request, CancellationToken cancellationToken)
    {
        if (request.ApplyToCategoryId.HasValue)
        {
            var categoryExists = await _categoryRepository.CategoryExistsAsync(request.ApplyToCategoryId.Value);
            if (!categoryExists)
                return Result<int>.Failure($"Danh mục với ID {request.ApplyToCategoryId} không tồn tại.");
        }

        if (request.ApplyToProductVariantId.HasValue)
        {
            var productVariantExists = await _productVariantRepository.ExistsAsync(request.ApplyToProductVariantId.Value);
            if (!productVariantExists)
                return Result<int>.Failure($"Sản phẩm biến thể với ID {request.ApplyToProductVariantId} không tồn tại.");
        }

        if (request.GiftProductVariantId.HasValue)
        {
            var giftExists = await _productVariantRepository.ExistsAsync(request.GiftProductVariantId.Value);
            if (!giftExists)
                return Result<int>.Failure($"Sản phẩm quà tặng với ID {request.GiftProductVariantId} không tồn tại.");
        }

        var rule = new PromotionRule
        {
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            ApplyToCategoryId = request.ApplyToCategoryId,
            ApplyToProductVariantId = request.ApplyToProductVariantId,
            GiftProductVariantId = request.GiftProductVariantId,
            MinQuantity = request.MinQuantity,
            FreeQuantity = request.FreeQuantity,
            DiscountPercentage = request.DiscountPercentage,
            Priority = request.Priority,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
        };

        await _promotionRuleRepository.AddAsync(rule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(rule.Id);
    }
}