using Microsoft.EntityFrameworkCore;
using EcommerceDemoV1.Domain.Entities;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Products.AnyAsync(p => p.Id == id);
    }

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Product> CreateAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        return product;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<IEnumerable<Product>> GetAllAsyncByAdmin(int page,
        int size)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .IgnoreQueryFilters()
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
    }


    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int size,
        string? category,
        decimal? minPrice,
        decimal? maxPrice)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category.Name == category);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.BasePrice >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.BasePrice <= maxPrice.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            throw new Exception("Product not found");

        product.IsDeleted = true;

        foreach (var variant in product.Variants)
        {
            variant.IsDeleted = true;
        }
    }

    public async Task UpdateAsync(Product product)
    {
        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        if (existingProduct != null)
        {
            existingProduct.Name = product.Name;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.BasePrice = product.BasePrice;
            existingProduct.Description = product.Description;
            existingProduct.ImageUrl = product.ImageUrl;
            _context.Products.Update(existingProduct);
        }
    }
}