using MediatR;
using EcommerceDemoV1.Application.Common;
using EcommerceDemoV1.Domain.Entities;
using EcommerceDemoV1.Domain.Enums;

namespace EcommerceDemoV1.Application.Features.Reviews.Commands.CreateReview;

public record CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Result<ReviewDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReviewCommandHandler(IProductRepository productRepository, IOrderRepository orderRepository, IReviewRepository reviewRepository, ICurrentUserService currentUserService, IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _reviewRepository = reviewRepository;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null)
            return Result<ReviewDto>.Failure("Product not found.");

        var userId = int.Parse(_currentUserService.UserId);
        var order = await _orderRepository.GetOrderCompletedAsync(userId, request.ProductId);
        if (!order.Any())
            return Result<ReviewDto>.Failure("You must have purchased this product to leave a review.");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Result<ReviewDto>.Failure("User not found.");

        var review = new Review
        {
            ProductId = request.ProductId,
            UserId = userId,
            OrderId = order.First(o => o.Items.Any(i => i.ProductVariant.ProductId == request.ProductId)).Id,
            User = user,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        var createdReview = await _reviewRepository.CreateReviewAsync(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var reviewDto = new ReviewDto(
            createdReview.Id,
            createdReview.UserId,
            createdReview.User.FullName,
            createdReview.Rating,
            createdReview.Comment,
            createdReview.CreatedAt
        );

        return Result<ReviewDto>.Success(reviewDto);
    }
}