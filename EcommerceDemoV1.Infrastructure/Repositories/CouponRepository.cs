using EcommerceDemoV1.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class CouponRepository : ICouponRepository
{
    private readonly AppDbContext _context;
    public CouponRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsExistingCodeAsync(string code)
    {
        return await _context.Coupons.AnyAsync(c => c.Code == code);
    }
    public async Task<Coupon?> GetByCodeAsync(string code)
    {
        return await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code && c.UsageLimit > 0 && c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow && c.IsActive);
    }

    public async Task<Coupon?> GetValidCouponByCodeAsync(string code, DateTime currentTime)
    {
        return await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code && c.UsageLimit > 0 && c.StartDate <= currentTime && c.EndDate >= currentTime && c.IsActive);
    }

    public async Task<List<Coupon>> GetAllAsync()
    {
        return await _context.Coupons
        .Where(c => c.EndDate >= DateTime.UtcNow && c.IsActive && c.UsageLimit > 0)
        .ToListAsync();
    }

    public async Task AddCouponAsync(Coupon coupon)
    {
        await _context.Coupons.AddAsync(coupon);
    }

    public async Task UpdateCouponAsync(Coupon coupon)
    {
        _context.Coupons.Update(coupon);
        await Task.CompletedTask;
    }
}