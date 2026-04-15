using Microsoft.EntityFrameworkCore;
using EcommerceDemoV1.Domain.Entities;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        return product.Id;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products.Where(p => !p.IsDeleted).ToListAsync();
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
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        if (existingProduct != null)
        {
            existingProduct.Name = product.Name;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.BasePrice = product.BasePrice;
            existingProduct.Description = product.Description;
            existingProduct.ImageUrl = product.ImageUrl;

            var updatedVariantIds = product.Variants.Select(v => v.Id).ToList();
            var variantsToRemove = existingProduct.Variants
                .Where(v => !updatedVariantIds.Contains(v.Id))
                .ToList();

            foreach (var variantToRemove in variantsToRemove)
            {
                variantToRemove.IsDeleted = true;
            }

            foreach (var incomingVariant in product.Variants)
            {
                var existingVariant = existingProduct.Variants.FirstOrDefault(v => v.Id == incomingVariant.Id && incomingVariant.Id != 0);

                if (existingVariant != null)
                {
                    existingVariant.SKU = incomingVariant.SKU;
                    existingVariant.Color = incomingVariant.Color;
                    existingVariant.Size = incomingVariant.Size;
                    existingVariant.Price = incomingVariant.Price;
                    existingVariant.StockQuantity = incomingVariant.StockQuantity;
                }
                else
                {
                    existingProduct.Variants.Add(incomingVariant);
                }
            }

            _context.Products.Update(existingProduct);
        }
    }
}