using System.Collections.Generic;

namespace EcommerceDemoV1.Application.DTOs.Product;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public decimal BasePrice { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    public List<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
}

public class ProductDtoRespone
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int CategoryId { get; set; }
    public decimal BasePrice { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    public List<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
}