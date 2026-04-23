using EcommerceDemoV1.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class PromotionRuleRepository : IPromotionRuleRepository
{
    private readonly AppDbContext _context;
    public PromotionRuleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PromotionRule>> GetActivePromotionRulesAsync(DateTime currentTime)
    {
        return await _context.PromotionRules
            .Include(pr => pr.Category)
            .Include(pr => pr.ProductVariant)
            .Include(x => x.GiftProductVariant)
            .Where(pr => pr.IsActive
                && pr.StartDate <= currentTime
                && pr.EndDate >= currentTime)
            .OrderByDescending(pr => pr.Priority)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(PromotionRule rule)
    {
        await _context.PromotionRules.AddAsync(rule);
    }
}
