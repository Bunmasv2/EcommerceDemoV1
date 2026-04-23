using EcommerceDemoV1.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Review> CreateReviewAsync(Review review)
    {
        var entry = await _context.Reviews.AddAsync(review);
        return entry.Entity;
    }

    public async Task<List<Review>> GetReviewsByProductIdAsync(int productId, int? page, int? size)
    {
        return await _context.Reviews
            .Where(r => r.ProductId == productId)
            .Include(vr => vr.Product)
                .ThenInclude(p => p.Variants)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Skip(((page ?? 1) - 1) * (size ?? 10))
            .Take(size ?? 10)
            .ToListAsync();
    }

    public async Task<bool> ReviewExistsAsync(int productId, int userId, int orderId)
    {
        return await _context.Reviews
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId && r.OrderId == orderId);
    }

    public async Task<(double AverageRating, int TotalReviews)> GetReviewSummaryByProductIdAsync(int productId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId)
            .ToListAsync();

        if (reviews.Count == 0)
            return (0, 0);

        double averageRating = reviews.Average(r => r.Rating);
        int totalReviews = reviews.Count;

        return (averageRating, totalReviews);
    }

    public async Task<bool> GetByProductUserOrderAsync(int productId, int userId, int orderId)
    {
        return await _context.Reviews
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId && r.OrderId == orderId);
    }
}