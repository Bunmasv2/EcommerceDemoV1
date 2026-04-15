namespace EcommerceDemoV1.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? CouponId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "PENDING";
    // PENDING | CONFIRMED | SHIPPING | COMPLETED | CANCELLED
    public string PaymentStatus { get; set; } = "PENDING_PAYMENT";
    // PENDING_PAYMENT | PAID | FAILED
    public string ShippingAddress { get; set; } = null!;
    public string? TrackingCode { get; set; }
    public string? PayOsOrderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Coupon? Coupon { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}