using EcommerceDemoV1.Domain.Entities;


public interface IProductRepository
{
    Task<bool> ExistsAsync(int id);
    Task<Product> CreateAsync(Product product);

    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsyncByAdmin(int page,
        int size);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int size,
        string? category,
        decimal? minPrice,
        decimal? maxPrice);

    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}
