using MediatR;

public record CreateReviewCommand(
    int ProductId,
    int Rating,
    string? Comment
) : IRequest<Result<ReviewDto>>;