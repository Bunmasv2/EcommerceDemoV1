using MediatR;
using EcommerceDemoV1.Domain.Entities;

public interface ICategoryRepository
{
    Task<bool> CategoryExistsAsync(int categoryId);
    // Task<Category> GetCategoryByIdAsync(int categoryId);
    // Task<List<Category>> GetAllCategoriesAsync();
    // Task<Category> CreateCategoryAsync(Category category);
    // Task UpdateCategoryAsync(Category category);
    // Task DeleteCategoryAsync(int categoryId);
}