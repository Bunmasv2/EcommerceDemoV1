namespace EcommerceDemoV1.Application.DTOs.Product;

public class ProductVariantDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string SKU { get; set; } = null!;
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

public class ProductVariantRequestDto
{
    public int ProductId { get; set; }
    public string SKU { get; set; } = null!;
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}