using MediatR;
using EcommerceDemoV1.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CategoryExistsAsync(int categoryId)
    {
        return await _context.Categories.AnyAsync(c => c.Id == categoryId);
    }

    // public async Task<Category> GetCategoryByIdAsync(int categoryId)
    // {
    //     return await _context.Categories.FindAsync(categoryId);
    // }

    // public async Task<List<Category>> GetAllCategoriesAsync()
    // {
    //     return await _context.Categories.ToListAsync();
    // }

    // public async Task<Category> CreateCategoryAsync(Category category)
    // {
    //     _context.Categories.Add(category);
    //     await _context.SaveChangesAsync();
    //     return category;
    // }

    // public async Task UpdateCategoryAsync(Category category)
    // {
    //     _context.Categories.Update(category);
    //     await _context.SaveChangesAsync();
    // }

    // public async Task DeleteCategoryAsync(int categoryId)
    // {
    //     var category = await _context.Categories.FindAsync(categoryId);
    //     if (category != null)
    //     {
    //         _context.Categories.Remove(category);
    //         await _context.SaveChangesAsync();
    //     }
    // }
}