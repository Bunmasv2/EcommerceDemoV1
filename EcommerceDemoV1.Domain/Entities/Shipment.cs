namespace EcommerceDemoV1.Domain.Entities;

public class Shipment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string? AhamoveOrderId { get; set; }
    public string Status { get; set; } = "CREATED";
    public string? TrackingUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Order Order { get; set; } = null!;
}
