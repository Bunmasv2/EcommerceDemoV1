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

    // Danh sách các phân loại đi kèm
    public List<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
}