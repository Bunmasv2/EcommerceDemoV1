using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using EcommerceDemoV1.Domain.Entities;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(v => v.Id);

        builder.HasIndex(v => v.SKU).IsUnique();

        builder.Property(v => v.SKU)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Color)
            .HasMaxLength(50);

        builder.Property(v => v.Size)
            .HasMaxLength(20);

        builder.Property(v => v.Price)
            .HasColumnType("decimal(18,2)");

        builder.HasQueryFilter(v => !v.IsDeleted);
    }
}