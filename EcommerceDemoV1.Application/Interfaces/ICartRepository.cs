using EcommerceDemoV1.Domain.Entities;

public interface ICartRepository
{
    Task<bool> CartExistsAsync(int cartId);
    Task<Cart?> GetCartByUserIdAsync(int userId, bool trackChanges = true);
    Task<Cart> CreateCartAsync(Cart cart);
    Task UpdateCartAsync(Cart cart);
    Task DeleteCartAsync(int cartId);
    Task UpdateCartItem(CartItem cartItem);
    Task AddCartItemAsync(CartItem cartItem);
}