namespace EcommerceDemoV1.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string? PayOsTransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "PENDING"; // PENDING | SUCCESS | FAILED
    public string Method { get; set; } = "PAYOS";
    public DateTime? PaidAt { get; set; }
    public Order Order { get; set; } = null!;
}