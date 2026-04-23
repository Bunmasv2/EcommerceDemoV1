using EcommerceDemoV1.Application.Common;
using MediatR;

public record GetReviewQuery(
    int ProductId,
    int? Page = 1,
    int? Size = 10
) : IRequest<Result<PagedResult<ReviewSummaryDto>>>;