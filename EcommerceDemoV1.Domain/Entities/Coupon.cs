namespace EcommerceDemoV1.Domain.Entities;

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = null!; // unique
    public string DiscountType { get; set; } = "PERCENTAGE"; // "PERCENTAGE" | "FIXED_AMOUNT"
    public decimal Value { get; set; }
    public decimal MinOrderValue { get; set; }
    public int UsageLimit { get; set; }
    public int UsedCount { get; set; } = 0;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}