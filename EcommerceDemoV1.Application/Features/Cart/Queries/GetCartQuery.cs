using EcommerceDemoV1.Application.DTOs.Cart;
using MediatR;

namespace EcommerceDemoV1.Application.Features.Cart.Queries.GetCart;

public record GetCartQuery : IRequest<CartDto>;