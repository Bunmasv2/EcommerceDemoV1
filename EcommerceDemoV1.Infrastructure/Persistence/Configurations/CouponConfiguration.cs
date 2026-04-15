using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EcommerceDemoV1.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasKey(c => c.Id);

        builder.HasIndex(c => c.Code).IsUnique();

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.DiscountType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Value)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.MinOrderValue)
            .HasColumnType("decimal(18,2)");
    }
}