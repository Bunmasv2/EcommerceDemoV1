namespace EcommerceDemoV1.Application.DTOs.Ahamove;

public record GeocodeAddressRequest(
    string Address
);

public record GeocodeAddressResponse(
    double Latitude,
    double Longitude,
    string Address
);

public record EstimateFeeRequest(
    double PickupLatitude,
    double PickupLongitude,
    string PickupAddress,
    double DeliveryLatitude,
    double DeliveryLongitude,
    string DeliveryAddress,
    double WeightKg,
    string? ServiceId = null
);

public record EstimateFeeResponse(
    string ServiceId,
    decimal TotalPay,
    double Distance,
    int EstimatedDuration
);

public record CreateOrderRequestDto(
    string OrderCode,
    double PickupLatitude,
    double PickupLongitude,
    string PickupAddress,
    double DeliveryLatitude,
    double DeliveryLongitude,
    string DeliveryAddress,
    double WeightKg,
    string DeliveryName,
    string DeliveryPhone,
    string? Note,
    string? ServiceId = null
);


public record CreateOrderResponseDto(
    string OrderId,
    string ServiceId,
    decimal TotalPay,
    double Distance,
    string SharedLink
);

/// <summary>
/// Webhook Payload từ Ahamove khi có cập nhật trạng thái
/// </summary>
public record AhamoveWebhookPayload(
    string OrderId,
    string Status,
    string? DriverName,
    string? DriverPhone,
    string? TrackingUrl,
    long UpdatedAt
);

/// <summary>
/// Items trong Webhook payload (nếu cần chi tiết)
/// </summary>
public record WebhookItem(
    string Name,
    int Quantity,
    double Weight
);
