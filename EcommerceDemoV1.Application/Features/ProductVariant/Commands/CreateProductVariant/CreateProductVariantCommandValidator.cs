using FluentValidation;

namespace EcommerceDemoV1.Application.Features.ProductVariant.Commands.CreateProductVariant;

public class CreateProductVariantCommandValidator : AbstractValidator<CreateProductVariantCommand>
{
    public CreateProductVariantCommandValidator()
    {
        RuleFor(x => x.Variants)
            .NotEmpty().WithMessage("At least one product variant is required.");
        RuleForEach(x => x.Variants).ChildRules(variant =>
        {
            variant.RuleFor(v => v.ProductId)
                .GreaterThan(0).WithMessage("Invalid Product ID.");

            variant.RuleFor(v => v.SKU)
                .NotEmpty().WithMessage("SKU is required.")
                .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters.");

            variant.RuleFor(v => v.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");

            variant.RuleFor(v => v.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");
        });
    }
}