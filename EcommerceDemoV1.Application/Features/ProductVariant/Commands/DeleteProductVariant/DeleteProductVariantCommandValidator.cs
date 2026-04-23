using FluentValidation;

namespace EcommerceDemoV1.Application.Features.ProductVariant.Commands.DeleteProductVariant;

public class DeleteProductVariantCommandValidator : AbstractValidator<DeleteProductVariantCommand>
{
    public DeleteProductVariantCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Variant ID is required to delete.");
    }
}