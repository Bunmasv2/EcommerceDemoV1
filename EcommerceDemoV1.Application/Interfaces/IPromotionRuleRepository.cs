using EcommerceDemoV1.Domain.Entities;
public interface IPromotionRuleRepository
{
    Task<List<PromotionRule>> GetActivePromotionRulesAsync(DateTime currentTime);
    Task AddAsync(PromotionRule rule);
}