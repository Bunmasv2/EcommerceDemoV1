using Microsoft.EntityFrameworkCore;
using EcommerceDemoV1.Domain.Entities;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> CartExistsAsync(int cartId)
    {
        return _context.Carts.AnyAsync(c => c.Id == cartId);
    }
    public async Task<Cart?> GetCartByUserIdAsync(int userId)
    {
        return await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(ci => ci.ProductVariant)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }
    public async Task<Cart> CreateCartAsync(Cart cart)
    {
        await _context.Carts.AddAsync(cart);
        return cart;
    }
    public async Task UpdateCartAsync(Cart cart)
    {
        _context.Carts.Update(cart);
    }
    public async Task DeleteCartAsync(int cartId)
    {
        var cart = await _context.Carts.FindAsync(cartId);
        if (cart != null)
        {
            _context.Carts.Remove(cart);
        }
    }

    public async Task UpdateCartItem(CartItem cartItem)
    {
        _context.CartItems.Update(cartItem);
    }

    public async Task AddCartItemAsync(CartItem cartItem)
    {
        await _context.CartItems.AddAsync(cartItem);
    }
}