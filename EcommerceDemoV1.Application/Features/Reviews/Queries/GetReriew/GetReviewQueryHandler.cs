using MediatR;
using EcommerceDemoV1.Application.Common;
using EcommerceDemoV1.Domain.Entities;
using EcommerceDemoV1.Domain.Enums;
using AutoMapper;

public class GetReviewQueryHandler : IRequestHandler<GetReviewQuery, Result<PagedResult<ReviewSummaryDto>>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetReviewQueryHandler(IReviewRepository reviewRepository, IUnitOfWork unitOfWork, IUserRepository userRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<ReviewSummaryDto>>> Handle(GetReviewQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetReviewsByProductIdAsync(request.ProductId, request.Page, request.Size);
        var (averageRating, totalReviews) = await _reviewRepository.GetReviewSummaryByProductIdAsync(request.ProductId);

        var reviewDtos = _mapper.Map<List<ReviewDto>>(reviews);

        var summary = new ReviewSummaryDto(
            request.ProductId,
            Math.Round(averageRating, 1),
            totalReviews,
            reviewDtos
        );
        var pagedResult = new PagedResult<ReviewSummaryDto>
        {
            Items = new List<ReviewSummaryDto> { summary },
            TotalCount = totalReviews,
            Page = request.Page ?? 1,
            Size = request.Size ?? 10
        };
        return Result<PagedResult<ReviewSummaryDto>>.Success(pagedResult);
    }
}