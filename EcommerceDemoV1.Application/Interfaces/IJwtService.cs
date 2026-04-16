using EcommerceDemoV1.Domain.Entities;

public interface IJwtService
{
    string GenerateToken(int userId, string email, string role);
}