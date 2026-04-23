using MediatR;
using EcommerceDemoV1.Application.Common;
using EcommerceDemoV1.Domain.Enums;

namespace EcommerceDemoV1.Application.Features.Orders.Commands.Checkout;

public record CheckoutCommand(string ShippingAddress, string ReceiverName, string ReceiverPhone, PaymentMethod PaymentMethod, string? CouponCode) : IRequest<Result<CheckoutResultDto>>;
