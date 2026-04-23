namespace EcommerceDemoV1.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // snapshot giá

    public Order Order { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;
}
