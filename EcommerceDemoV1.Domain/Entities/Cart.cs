namespace EcommerceDemoV1.Domain.Entities;

public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<CartItem> Items { get; set; } = new List<CartItem>();
    public string? AppliedCouponCode { get; set; }
    public decimal AppliedDiscount { get; set; }

    public User User { get; set; } = null!;
}