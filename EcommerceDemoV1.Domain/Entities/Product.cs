namespace EcommerceDemoV1.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; } // giá tham chiếu
    public string? ImageUrl { get; set; }
    public bool IsDeleted { get; set; } = false; // soft delete
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Category Category { get; set; } = null!;
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string SKU { get; set; } = null!;
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsDeleted { get; set; } = false;

    public Product Product { get; set; } = null!;
}