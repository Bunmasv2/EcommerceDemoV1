namespace EcommerceDemoV1.Domain.Entities;

public class Shipment
{
    public int Id { get; set; }
    public int OrderId { get; set; }

    // Thông tin định danh từ AhaMove
    public string? AhamoveOrderId { get; set; }
    public string? ServiceId { get; set; } // VD: "SGN-BIKE", "SGN-EXPRESS"
    public string Status { get; set; } = "IDLE";
    public string? TrackingUrl { get; set; }

    public decimal ShippingFee { get; set; }
    public double? Distance { get; set; } // Ánh xạ từ 'distance' (km)

    public double DeliveryLatitude { get; set; }
    public double DeliveryLongitude { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } // Lưu vết thời gian nhận Webhook cuối cùng

    // Navigation Property
    public Order Order { get; set; } = null!;
}
