public class CheckoutResultDto
{
    public int OrderId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? PaymentUrl { get; set; }
}

public class PreviewItemDto
{
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    public bool IsGift { get; set; } // Phân biệt hàng mua và quà tặng
}

public class PreviewCheckoutDto
{
    public decimal SubTotal { get; set; }
    public decimal PromotionDiscount { get; set; }
    public decimal RankDiscount { get; set; }
    public decimal CouponDiscount { get; set; }
    public decimal FinalTotal { get; set; }

    public List<PreviewItemDto> Items { get; set; } = new List<PreviewItemDto>();
}