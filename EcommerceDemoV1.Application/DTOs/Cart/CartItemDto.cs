using EcommerceDemoV1.Application.DTOs.Product;

namespace EcommerceDemoV1.Application.DTOs.Cart;

public class CartDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public List<CartItemDto> Items { get; set; } = new();

    public decimal TotalPrice => Items.Sum(i => i.Quantity * i.ProductVariant.Price);
}

public class CartItemDto
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; }

    public ProductVariantDto ProductVariant { get; set; } = null!;
}