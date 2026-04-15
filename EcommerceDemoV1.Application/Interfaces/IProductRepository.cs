using EcommerceDemoV1.Domain.Entities;


public interface IProductRepository
{
    Task<int> CreateAsync(Product product);

    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int size,
        string? category,
        decimal? minPrice,
        decimal? maxPrice);

    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}
