using EcommerceDemoV1.Application.DTOs.Cart;
using MediatR;
using EcommerceDemoV1.Application.DTOs.Product;
using EcommerceDemoV1.Domain.Services;

namespace EcommerceDemoV1.Application.Features.Cart.Queries.GetCart;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCartQueryHandler(ICartRepository cartRepository, IUserRepository userRepository, ICurrentUserService currentUserService)
    {
        _cartRepository = cartRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException();

        var cart = await _cartRepository.GetCartByUserIdAsync(int.Parse(userId));
        var user = await _userRepository.GetByIdAsync(int.Parse(userId));

        if (cart == null || user == null) return new CartDto();

        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId.ToString(),
            MemberRank = user.MemberRank.ToString(),
            RankDiscountRate = RankService.GetDiscountRate(user.MemberRank),
            Items = cart.Items.Select(i => new CartItemDto
            {
                Id = i.Id,
                CartId = i.CartId,
                ProductVariantId = i.ProductVariantId,
                Quantity = i.Quantity,
                ProductVariant = new ProductVariantDto
                {
                    Id = i.ProductVariant.Id,
                    ProductId = i.ProductVariant.ProductId,
                    SKU = i.ProductVariant.SKU,
                    Price = i.ProductVariant.Price,
                    Color = i.ProductVariant.Color,
                    Size = i.ProductVariant.Size
                }
            }).ToList()
        };
    }
}