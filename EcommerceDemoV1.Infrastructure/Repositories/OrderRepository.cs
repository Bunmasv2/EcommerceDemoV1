using EcommerceDemoV1.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.ProductVariant)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetOrderByUserIdAsync(int userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        return order;
    }
}