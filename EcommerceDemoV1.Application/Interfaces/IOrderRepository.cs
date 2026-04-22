using EcommerceDemoV1.Domain.Entities;
using EcommerceDemoV1.Domain.Enums;


public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int Id);
    Task<List<Order>> GetOrderByUserIdAsync(int userId);
    Task<Order> CreateOrderAsync(Order order);
    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(int userId, int page, int size, string? search = null);
    Task<List<Order>> GetExpiredPendingOrdersAsync(DateTime expiredTime);
    Task<List<Order>> GetOrderCompletedAsync(int userId, int productId);
    Task<Order?> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
    // Task<bool> ReviewExistsAsync(int productId, int userId, int orderId);
}