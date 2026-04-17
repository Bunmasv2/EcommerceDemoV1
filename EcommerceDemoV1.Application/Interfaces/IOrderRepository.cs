using EcommerceDemoV1.Domain.Entities;


public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int Id);
    Task<List<Order>> GetOrderByUserIdAsync(int userId);
    Task<Order> CreateOrderAsync(Order order);
}