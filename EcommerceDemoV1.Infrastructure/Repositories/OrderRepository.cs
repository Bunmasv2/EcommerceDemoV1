using EcommerceDemoV1.Domain.Entities;
using EcommerceDemoV1.Domain.Enums;
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

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(int userId, int page, int size, string? search = null)
    {
        var query = _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            bool isStatusSearch = Enum.TryParse<OrderStatus>(search, true, out var searchStatus);

            query = query.Where(o =>
                o.Id.ToString().Contains(search) ||
                (isStatusSearch && o.Status == searchStatus));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Order>> GetExpiredPendingOrdersAsync(DateTime expiredTime)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .Where(o => o.Status == OrderStatus.Pending && o.PaymentStatus == PaymentStatus.Pending && o.CreatedAt < expiredTime && o.Payments.Any(p => p.Method == PaymentMethod.PayOS || p.Method == PaymentMethod.VNPay))
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrderCompletedAsync(int userId, int productId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId && o.Status == OrderStatus.Completed && o.Items.Any(i => i.ProductVariant.ProductId == productId))
            .Include(o => o.Items)
                .ThenInclude(i => i.ProductVariant)
            .OrderByDescending(o => o.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Order> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return null;

        order.Status = newStatus;
        _context.Orders.Update(order);
        return order;
    }

    public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.Shipments)
            .Include(o => o.Items)
                .ThenInclude(i => i.ProductVariant)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }
}