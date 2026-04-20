using EcommerceDemoV1.Domain.Entities;

public interface IReviewRepository
{
    Task<Review> CreateReviewAsync(Review review);
    Task<List<Review>> GetReviewsByProductIdAsync(int productId, int? page, int? size);
    Task<(double AverageRating, int TotalReviews)> GetReviewSummaryByProductIdAsync(int productId);
}