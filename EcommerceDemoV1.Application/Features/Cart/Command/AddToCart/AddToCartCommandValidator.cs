using FluentValidation;

namespace EcommerceDemoV1.Application.Features.Cart.Command.AddToCart;

public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public Guid UserId { get; set; }
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .NotEmpty().WithMessage("Product variant ID is required.")
            .GreaterThan(0).WithMessage("Invalid product variant ID.");
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.");
    }
}