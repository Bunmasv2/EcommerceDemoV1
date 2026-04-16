using MediatR;
using EcommerceDemoV1.Application.DTOs.Cart;
using EcommerceDemoV1.Application.DTOs.Product;

namespace EcommerceDemoV1.Application.Features.Cart.Command.AddToCart;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, Result<CartItemDto>>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly ICurrentUserService _currentUserService;

    public AddToCartCommandHandler(ICartRepository cartRepository, IUnitOfWork unitOfWork, IProductVariantRepository productVariantRepository, ICurrentUserService currentUserService)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
        _productVariantRepository = productVariantRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CartItemDto>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Result<CartItemDto>.Failure("User is not authenticated.");

        var productVariant = await _productVariantRepository.GetByIdAsync(request.ProductVariantId);
        if (productVariant == null)
        {
            return Result<CartItemDto>.Failure("Biến thể sản phẩm không tồn tại.");
        }
        if (productVariant.StockQuantity < request.Quantity)
            return Result<CartItemDto>.Failure("Not enough stock available.");

        var cart = await _cartRepository.GetCartByUserIdAsync(int.Parse(userId));
        if (cart == null)
        {
            cart = new EcommerceDemoV1.Domain.Entities.Cart { UserId = int.Parse(userId) };
            await _cartRepository.CreateCartAsync(cart);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductVariantId == request.ProductVariantId);
        EcommerceDemoV1.Domain.Entities.CartItem cartItem;
        if (existingItem != null)
        {
            if (productVariant.StockQuantity < (existingItem.Quantity + request.Quantity))
                return Result<CartItemDto>.Failure("Total quantity in cart exceeds stock.");
            existingItem.Quantity += request.Quantity;
            cartItem = existingItem;
            await _cartRepository.UpdateCartItem(existingItem);
        }
        else
        {
            cartItem = new EcommerceDemoV1.Domain.Entities.CartItem
            {
                CartId = cart.Id,
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity
            };

            await _cartRepository.AddCartItemAsync(cartItem);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CartItemDto>.Success(new CartItemDto
        {
            Id = cartItem.Id,
            CartId = cartItem.CartId,
            ProductVariantId = cartItem.ProductVariantId,
            Quantity = cartItem.Quantity,
            ProductVariant = new ProductVariantDto
            {
                Id = cartItem.ProductVariant.Id,
                ProductId = cartItem.ProductVariantId,
                SKU = cartItem.ProductVariant.SKU,
                Color = cartItem.ProductVariant.Color,
                Size = cartItem.ProductVariant.Size,
                Price = cartItem.ProductVariant.Price,
                StockQuantity = cartItem.ProductVariant.StockQuantity
            }
        });
    }
}