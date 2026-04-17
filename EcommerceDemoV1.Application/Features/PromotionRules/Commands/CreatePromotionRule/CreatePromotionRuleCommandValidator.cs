using FluentValidation;
using EcommerceDemoV1.Domain.Enums;

namespace EcommerceDemoV1.Application.Features.PromotionRules.Commands.CreatePromotionRule;

public class CreatePromotionRuleCommandValidator : AbstractValidator<CreatePromotionRuleCommand>
{
    public CreatePromotionRuleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate)
            .WithMessage("Ngày bắt đầu phải nhỏ hơn ngày kết thúc.");

        // Rule cho "Mua X Tặng Y"
        When(x => x.Type == PromotionType.BuyXGetYFree, () =>
        {
            RuleFor(x => x.ApplyToProductVariantId).NotNull()
                .WithMessage("Phải chọn sản phẩm cụ thể cho khuyến mãi Mua X Tặng Y.");
            RuleFor(x => x.GiftProductVariantId).NotNull()
                .WithMessage("Phải chọn sản phẩm quà tặng (Y) cụ thể.");
            RuleFor(x => x.MinQuantity).GreaterThan(0);
            RuleFor(x => x.FreeQuantity).GreaterThan(0);
        });

        // Rule cho "Giảm giá theo danh mục"
        When(x => x.Type == PromotionType.CategoryDiscount, () =>
        {
            RuleFor(x => x.ApplyToCategoryId).NotNull()
                .WithMessage("Phải chọn danh mục cho khuyến mãi này.");
            RuleFor(x => x.DiscountPercentage).ExclusiveBetween(0, 100)
                .WithMessage("Phần trăm giảm giá phải lớn hơn 0 và nhỏ hơn 100.");
        });

        // Rule cho "Giảm giá sản phẩm"
        When(x => x.Type == PromotionType.ProductDiscount, () =>
        {
            RuleFor(x => x.ApplyToProductVariantId).NotNull()
                .WithMessage("Phải chọn sản phẩm cụ thể.");
            RuleFor(x => x.DiscountPercentage).ExclusiveBetween(0, 100);
        });
    }
}