namespace EcommerceDemoV1.Domain.Entities;

public class PromotionRule
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string RuleType { get; set; } = null!; // BUY_X_GET_Y | CATEGORY_DISCOUNT
    public int Priority { get; set; }
    public string ConditionJson { get; set; } = "{}"; // serialize condition
    public string ActionJson { get; set; } = "{}";   // serialize action
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}