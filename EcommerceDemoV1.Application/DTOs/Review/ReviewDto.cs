public record ReviewDto(
    int Id,
    int UserId,
    string UserName,
    int Rating,
    string? Comment,
    DateTime CreatedAt
);

public record ReviewSummaryDto(
    int ProductId,
    double AverageRating,
    int TotalReviews,
    List<ReviewDto> Reviews
);