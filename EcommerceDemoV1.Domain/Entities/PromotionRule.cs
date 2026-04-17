using EcommerceDemoV1.Domain.Enums;
namespace EcommerceDemoV1.Domain.Entities;

public class PromotionRule
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public PromotionType Type { get; set; }

    public int? ApplyToCategoryId { get; set; }
    public int? ApplyToProductVariantId { get; set; }
    public int? GiftProductVariantId { get; set; }

    public int MinQuantity { get; set; }
    public int FreeQuantity { get; set; }

    public decimal DiscountPercentage { get; set; }

    public int Priority { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public Category? Category { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public ProductVariant? GiftProductVariant { get; set; }
}