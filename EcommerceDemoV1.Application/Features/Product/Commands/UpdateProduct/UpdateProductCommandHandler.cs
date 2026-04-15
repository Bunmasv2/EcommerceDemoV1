using MediatR;


namespace EcommerceDemoV1.Application.Features.Product.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        // Map data để gửi xuống Repository xử lý logic update
        var productUpdate = new EcommerceDemoV1.Domain.Entities.Product
        {
            Id = request.Id,
            Name = request.Name,
            CategoryId = request.CategoryId,
            BasePrice = request.BasePrice,
            Description = request.Description,
            ImageUrl = request.ImageUrl
            // Variants = request.Variants.Select(v => new EcommerceDemoV1.Domain.Entities.ProductVariant
            // {
            //     Id = v.Id, // Quan trọng: Phải truyền Id để Repo biết là Cập nhật hay Thêm mới
            //     SKU = v.SKU,
            //     Color = v.Color,
            //     Size = v.Size,
            //     Price = v.Price,
            //     StockQuantity = v.StockQuantity
            // }).ToList()
        };

        await _productRepository.UpdateAsync(productUpdate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}