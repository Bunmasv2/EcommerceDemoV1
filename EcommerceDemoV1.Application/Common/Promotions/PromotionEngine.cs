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

        // Tạo Hash Map cho giỏ hàng để tra cứu O(1)
        var itemsByVariantId = cartItems.ToDictionary(i => i.ProductVariantId);

        //Nhóm các sản phẩm theo CategoryId
        var itemsByCategoryId = cartItems
            .Where(i => i.ProductVariant?.Product?.CategoryId != null)
            .GroupBy(i => i.ProductVariant.Product.CategoryId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var rule in activeRules)
        {
            var currentResult = new PromotionCalculationResult();
            decimal currentBenefitValue = 0;

            //Dùng thử Rule Giảm giá theo Danh mục
            if (rule.Type == PromotionType.CategoryDiscount && rule.ApplyToCategoryId.HasValue)
            {
                if (itemsByCategoryId.TryGetValue(rule.ApplyToCategoryId.Value, out var targetItems))
                {
                    foreach (var item in targetItems)
                    {
                        decimal discount = (item.ProductVariant.Price * item.Quantity) * (rule.DiscountPercentage / 100);
                        currentResult.TotalDiscount += discount;
                        currentResult.AppliedDescriptions.Add($"Giảm {rule.DiscountPercentage}% cho danh mục {rule.Category?.Name}");
                    }
                    currentBenefitValue = currentResult.TotalDiscount;
                }
            }

            //Dùng thử Rule Mua X tặng Y
            else if (rule.Type == PromotionType.BuyXGetYFree && rule.ApplyToProductVariantId.HasValue)
            {
                if (itemsByVariantId.TryGetValue(rule.ApplyToProductVariantId.Value, out var item) && item.Quantity >= rule.MinQuantity)
                {
                    int giftCount = (item.Quantity / rule.MinQuantity) * rule.FreeQuantity;
                    int giftId = rule.GiftProductVariantId.Value;

                    currentResult.Gifts.Add(new GiftItem { ProductVariantId = giftId, Quantity = giftCount });
                    currentResult.AppliedDescriptions.Add($"Khuyến mãi {rule.Name}: Tặng {giftCount} sản phẩm cùng loại");

                    decimal giftPrice = rule.GiftProductVariant != null ? rule.GiftProductVariant.Price : 0;
                    currentBenefitValue = giftCount * giftPrice;
                }
            }

            // Nếu Rule hiện tại mang lại lợi ích quy ra tiền LỚN HƠN Rule tốt nhất trước đó
            if (currentBenefitValue > maxBenefitValue)
            {
                maxBenefitValue = currentBenefitValue;
                bestResult = currentResult;
            }
        }

        return bestResult;
    }
}