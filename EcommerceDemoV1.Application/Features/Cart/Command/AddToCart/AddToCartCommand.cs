using MediatR;
using EcommerceDemoV1.Application.Common;
using EcommerceDemoV1.Application.DTOs.Cart;

namespace EcommerceDemoV1.Application.Features.Cart.Command.AddToCart;

public record AddToCartCommand(int ProductVariantId, int Quantity) : IRequest<Result<CartItemDto>>;