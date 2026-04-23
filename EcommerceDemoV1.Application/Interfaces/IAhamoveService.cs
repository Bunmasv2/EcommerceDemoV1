using EcommerceDemoV1.Domain.Common;

public interface IAhamoveService
{
    Task<Location> GeocodeAddressAsync(string address);
    Task<EstimateResponse> EstimateOrderFeeAsync(
        Location pickupLocation,
        Location deliveryLocation,
        double weightKg,
        string? serviceId = null);
    Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderStatusResponse> GetOrderStatusAsync(string ahamoveOrderId);
    Task<IEnumerable<EcommerceDemoV1.Domain.Entities.Shipment>> GetShipmentsByAhamoveOrderIdAsync(string ahamoveOrderId);
}

public record EstimateResponse(
    string ServiceId,
    decimal TotalPay,
    double Distance,
    int EstimatedDuration
);

public record CreateOrderRequest(
    string OrderCode,
    double WeightKg,
    Location PickupLocation,
    Location DeliveryLocation,
    string DeliveryName,
    string DeliveryPhone,
    string? Note,
    string? ServiceId = null,
    decimal CodAmount = 0
);

public record CreateOrderResponse(
    string OrderId,
    string status,
    string ServiceId,
    decimal TotalPay,
    double Distance,
    string SharedLink
);

public record OrderStatusResponse(
    string order_id,
    string status,
    string? DriverName,
    string? DriverPhone,
    string? TrackingUrl
);
