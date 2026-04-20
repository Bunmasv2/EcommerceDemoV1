using MediatR;

namespace EcommerceDemoV1.Application.Features.Payment.Commands.CreatePayOsLink;

public record CreatePayOsLinkCommand(int orderId) : IRequest<Result<string>>;