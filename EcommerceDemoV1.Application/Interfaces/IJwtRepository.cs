using EcommerceDemoV1.Domain.Entities;

public interface IJwtRepository
{
    string GenerateToken(int userId, string email, string role);
}