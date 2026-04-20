using EcommerceDemoV1.Application.Common;
using EcommerceDemoV1.Domain.Entities;
using MediatR;

namespace EcommerceDemoV1.Application.Features.Orders.Queries.GetOrders;

public class GetOrderCommandHandler : IRequestHandler<GetOrderCommand, PagedResult<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;


    public GetOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, IUserRepository userRepository, ICurrentUserService currentUserService)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(_currentUserService.UserId);
        if (string.IsNullOrEmpty(userId.ToString()))
            return new PagedResult<OrderDto>
            {
                Items = new List<OrderDto>(),
                TotalCount = 0,
                Page = request.Page,
                Size = request.Size
            };

        var (items, totalCount) = await _orderRepository.GetPagedAsync(
            userId,
            request.Page,
            request.Size,
            request.Search);
        var result = new List<OrderDto>();
        foreach (var order in items)
        {
            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.SubTotal,
                FinalTotal = order.FinalTotal,
                CouponCode = order.CouponCode,
                CreatedAt = order.CreatedAt,
                Status = order.Status.ToString(),
                Items = order.Items.Select(i => new OrderItemDto
                {
                    ProductVariantId = i.ProductVariantId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
            result.Add(orderDto);
        }

        return new PagedResult<OrderDto>
        {
            Items = result,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        };
    }
}