using EcommerceDemoV1.Domain.Enums;

namespace EcommerceDemoV1.Domain.Services;

public static class RankService
{
    public const decimal MoneyPerPoint = 100000m;

    public static int CalculatePoints(decimal orderTotal)
        => (int)(orderTotal / MoneyPerPoint);
    // Quy tắc đổi điểm sang hạng
    public static MemberRank CalculateRank(int points) => points switch
    {
        >= 5000 => MemberRank.Diamond,
        >= 2000 => MemberRank.Gold,
        >= 500 => MemberRank.Silver,
        _ => MemberRank.Bronze
    };

    // Quy tắc chiết khấu theo hạng
    public static decimal GetDiscountRate(MemberRank rank) => rank switch
    {
        MemberRank.Silver => 0.02m,  // 2%
        MemberRank.Gold => 0.05m,    // 5%
        MemberRank.Diamond => 0.10m, // 10%
        _ => 0m                      // Bronze 0%
    };
}