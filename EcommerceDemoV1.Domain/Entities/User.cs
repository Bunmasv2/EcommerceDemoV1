using EcommerceDemoV1.Domain.Enums;

namespace EcommerceDemoV1.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string Role { get; set; } = "User"; // "User" | "Admin"
    public int LoyaltyPoints { get; set; } = 0;
    public MemberRank MemberRank { get; set; } = MemberRank.Bronze; // Bronze | Silver | Gold | Diamond
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public Cart? Cart { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}