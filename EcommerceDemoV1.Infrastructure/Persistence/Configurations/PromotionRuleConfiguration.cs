using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EcommerceDemoV1.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class PromotionRuleConfiguration : IEntityTypeConfiguration<PromotionRule>
{
    public void Configure(EntityTypeBuilder<PromotionRule> builder)
    {
        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pr => pr.RuleType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pr => pr.ConditionJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(pr => pr.ActionJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");
    }
}