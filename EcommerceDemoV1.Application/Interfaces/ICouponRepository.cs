using EcommerceDemoV1.Domain.Entities;

public interface ICouponRepository
{
    Task<bool> IsExistingCodeAsync(string code);
    Task<Coupon?> GetByCodeAsync(string code);
    Task<List<Coupon>> GetAllAsync();
    Task<Coupon?> GetValidCouponByCodeAsync(string code, DateTime currentTime);
    Task AddCouponAsync(Coupon coupon);
    Task UpdateCouponAsync(Coupon coupon);
}