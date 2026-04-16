

using EcommerceDemoV1.Domain.Entities;

public interface IProductVariantRepository
{
    Task<bool> ExistsAsync(int id);
    Task<ProductVariant?> GetByIdAsync(int id);
    Task<IEnumerable<ProductVariant>> GetAllAsyncByAdmin(int page, int size);
    Task<(IReadOnlyList<ProductVariant> Items, int TotalCount)> GetPagedAsync(
        int page,
        int size,
        string? category,
        decimal? minPrice,
        decimal? maxPrice);

    Task<ProductVariant> CreateAsync(ProductVariant productVariant);
    Task<List<ProductVariant>> CreateRangeAsync(List<ProductVariant> productVariants);

    Task UpdateAsync(ProductVariant productVariant);
    Task DeleteAsync(int id);
}
