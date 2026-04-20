using EcommerceDemoV1.Domain.Entities;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<string?> GetUserNameByIdAsync(int id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);


}