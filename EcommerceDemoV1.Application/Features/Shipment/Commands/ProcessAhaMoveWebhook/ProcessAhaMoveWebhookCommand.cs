using MediatR;
using EcommerceDemoV1.Application.Common;
using System.Text.Json.Serialization;

namespace EcommerceDemoV1.Application.Features.Shipment.Commands.ProcessAhaMoveWebhook;

public record ProcessAhaMoveWebhookCommand(
    [property: JsonPropertyName("_id")] string AhamoveOrderId,

    [property: JsonPropertyName("status")] string Status,

    // AhaMove trả về tên và SĐT tài xế dưới dạng supplier_name và supplier_phone
    [property: JsonPropertyName("supplier_name")] string? DriverName,

    [property: JsonPropertyName("supplier_phone")] string? DriverPhone,

    [property: JsonPropertyName("shared_link")] string? TrackingUrl
) : IRequest<Result<WebhookProcessResponse>>;

public record WebhookProcessResponse(
    bool Success,
    string Message,
    int OrderId
);