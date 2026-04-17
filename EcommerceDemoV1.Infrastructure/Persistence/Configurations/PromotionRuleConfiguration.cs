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

        builder.Property(pr => pr.Description)
            .HasMaxLength(500);

        builder.Property(pr => pr.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(pr => pr.DiscountPercentage)
            .HasPrecision(5, 2);

        builder.HasOne(pr => pr.Category)
            .WithMany()
            .HasForeignKey(pr => pr.ApplyToCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(pr => pr.ProductVariant)
            .WithMany()
            .HasForeignKey(pr => pr.ApplyToProductVariantId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(pr => pr.GiftProductVariant)
            .WithMany()
            .HasForeignKey(pr => pr.GiftProductVariantId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}