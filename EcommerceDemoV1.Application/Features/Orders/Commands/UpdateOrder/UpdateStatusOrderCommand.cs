using MediatR;
using EcommerceDemoV1.Domain.Enums;
using EcommerceDemoV1.Domain.Entities;

namespace EcommerceDemoV1.Application.Features.Orders.Commands.UpdateOrder;

public record UpdateStatusOrderCommand(int orderId, OrderStatus newStatus) : IRequest<Result<string>>;
