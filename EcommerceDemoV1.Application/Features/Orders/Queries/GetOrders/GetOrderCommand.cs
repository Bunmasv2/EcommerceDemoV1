using MediatR;
using EcommerceDemoV1.Domain.Entities;
using EcommerceDemoV1.Application.Common;

namespace EcommerceDemoV1.Application.Features.Orders.Queries.GetOrders;

public record GetOrderCommand(
    int Page,
    int Size,
    string? Search = null
) : IRequest<PagedResult<OrderDto>>;