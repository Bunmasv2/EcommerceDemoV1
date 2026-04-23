using EcommerceDemoV1.Domain.Enums;

namespace EcommerceDemoV1.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }

    public string? PayOsOrderId { get; set; }
    public string? PayOsTransactionId { get; set; }

    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentMethod Method { get; set; } = PaymentMethod.PayOS;
    public DateTime? PaidAt { get; set; }
    public Order Order { get; set; } = null!;
}