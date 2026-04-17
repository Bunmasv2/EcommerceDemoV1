using EcommerceDemoV1.Domain.Enums;

namespace EcommerceDemoV1.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? CouponId { get; set; }
    public string? CouponCode { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PromotionDiscount { get; set; } // Khuyến mãi (Task 8)
    public decimal RankDiscount { get; set; }      // Chiết khấu hạng (Task 5.3)
    public decimal CouponDiscount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal FinalTotal { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

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