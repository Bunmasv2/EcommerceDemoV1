using EcommerceDemoV1.Application.DTOs.Product;

namespace EcommerceDemoV1.Application.DTOs.Cart;

public class CartDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public List<CartItemDto> Items { get; set; } = new();

    public string MemberRank { get; set; } = string.Empty; // Hiển thị "Gold", "Silver"...
    public decimal RankDiscountRate { get; set; } // Ví cập: 0.05 (5%)

    // Tổng tiền gốc
    public decimal SubTotal => Items.Sum(i => i.Quantity * i.ProductVariant.Price);

    // Số tiền giảm giá theo hạng thành viên
    public decimal RankDiscountAmount => SubTotal * RankDiscountRate;

    // Tổng tiền sau khi đã trừ ưu đãi hạng (Dùng để Checkout)
    public decimal TotalAfterRankDiscount => SubTotal - RankDiscountAmount;

    // If use Coupon (Task 5.2)
    public string? AppliedCouponCode { get; set; }
    public decimal CouponDiscountAmount { get; set; }

    public decimal FinalTotalPrice => TotalAfterRankDiscount - CouponDiscountAmount;
}

public class CartItemDto
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; }

    public ProductVariantDto ProductVariant { get; set; } = null!;
}