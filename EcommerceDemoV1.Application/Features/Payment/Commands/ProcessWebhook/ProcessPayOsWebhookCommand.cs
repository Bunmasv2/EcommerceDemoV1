using System.Text.Json;
using MediatR;

namespace EcommerceDemoV1.Application.Features.Payment.Commands.ProcessWebhook;

public record ProcessPayOsWebhookCommand(JsonElement WebhookBody) : IRequest<Result<string>>;
