using EcommerceDemoV1.Domain.Entities;
using EcommerceDemoV1.Domain.Enums;

public class PromotionCalculationResult
{
    public decimal TotalDiscount { get; set; }
    public List<string> AppliedDescriptions { get; set; } = new();
    public List<GiftItem> Gifts { get; set; } = new();
}

public class GiftItem { public int ProductVariantId { get; set; } public int Quantity { get; set; } }

public class PromotionEngine
{
    public PromotionCalculationResult ApplyBestRule(List<CartItem> cartItems, List<PromotionRule> activeRules)
    {
        var bestResult = new PromotionCalculationResult();
        decimal maxBenefitValue = 0;

        foreach (var rule in activeRules)
        {
            var currentResult = new PromotionCalculationResult();
            decimal currentBenefitValue = 0;

            // 1. Dùng thử Rule Giảm giá theo Danh mục
            if (rule.Type == PromotionType.CategoryDiscount && rule.ApplyToCategoryId.HasValue)
            {
                var targetItems = cartItems.Where(i => i.ProductVariant.Product.CategoryId == rule.ApplyToCategoryId).ToList();
                foreach (var item in targetItems)
                {
                    decimal discount = (item.ProductVariant.Price * item.Quantity) * (rule.DiscountPercentage / 100);
                    currentResult.TotalDiscount += discount;
                    currentResult.AppliedDescriptions.Add($"Giảm {rule.DiscountPercentage}% cho danh mục {rule.Category?.Name}");
                }

                // Giá trị của Rule này chính là số tiền được giảm
                currentBenefitValue = currentResult.TotalDiscount;
            }

            // 2. Dùng thử Rule Mua X tặng Y
            else if (rule.Type == PromotionType.BuyXGetYFree && rule.ApplyToProductVariantId.HasValue)
            {
                var item = cartItems.FirstOrDefault(i => i.ProductVariantId == rule.ApplyToProductVariantId);
                if (item != null && item.Quantity >= rule.MinQuantity)
                {
                    int giftCount = (item.Quantity / rule.MinQuantity) * rule.FreeQuantity;
                    int giftId = rule.GiftProductVariantId.Value;
                    currentResult.Gifts.Add(new GiftItem { ProductVariantId = giftId, Quantity = giftCount });
                    currentResult.AppliedDescriptions.Add($"Khuyến mãi {rule.Name}: Tặng {giftCount} sản phẩm cùng loại");

                    // Định giá món quà để đem đi so sánh (Số quà * Giá gốc của sản phẩm đó)
                    decimal giftPrice = rule.GiftProductVariant != null ? rule.GiftProductVariant.Price : 0;
                    currentBenefitValue = giftCount * giftPrice;
                }
            }

            // Nếu Rule hiện tại mang lại lợi ích quy ra tiền LỚN HƠN Rule tốt nhất trước đó
            if (currentBenefitValue > maxBenefitValue)
            {
                maxBenefitValue = currentBenefitValue;
                bestResult = currentResult; // Ghi đè kết quả tốt nhất
            }
        }

        return bestResult;
    }
}