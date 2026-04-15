using MediatR;

namespace EcommerceDemoV1.Application.Features.Product.Commands.DeleteProduct;

public record DeleteProductCommand(int Id) : IRequest<Unit>;