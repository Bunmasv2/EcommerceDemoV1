using Microsoft.EntityFrameworkCore;
using EcommerceDemoV1.Domain.Entities;

public class ProductVariantRepository : IProductVariantRepository
{
    private readonly AppDbContext _context;

    public ProductVariantRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.ProductVariants.AnyAsync(p => p.Id == id);
    }

    public async Task<ProductVariant> CreateAsync(ProductVariant variant)
    {
        await _context.ProductVariants.AddAsync(variant);
        return variant;
    }

    public async Task<List<ProductVariant>> CreateRangeAsync(List<ProductVariant> productVariants)
    {
        await _context.ProductVariants.AddRangeAsync(productVariants);
        return productVariants;
    }

    public async Task<ProductVariant?> GetByIdAsync(int id)
    {
        return await _context.ProductVariants.FindAsync(id);
    }

    public async Task<IEnumerable<ProductVariant>> GetAllAsyncByAdmin(int page, int size)
    {
        return await _context.ProductVariants
            .IgnoreQueryFilters()
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
    }


    public async Task<(IReadOnlyList<ProductVariant> Items, int TotalCount)> GetPagedAsync(
        int page,
        int size,
        string? category,
        decimal? minPrice,
        decimal? maxPrice)
    {
        var query = _context.ProductVariants
            .Include(v => v.Product)
            .Where(v => !v.IsDeleted);

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Product.Category.Name == category);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Product.BasePrice >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Product.Name)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task DeleteAsync(int id)
    {
        var variant = await GetByIdAsync(id);

        if (variant == null)
            throw new Exception("Variant not found");

        variant.IsDeleted = true;
    }

    public async Task UpdateAsync(ProductVariant variant)
    {
        _context.ProductVariants.Update(variant);
    }
}