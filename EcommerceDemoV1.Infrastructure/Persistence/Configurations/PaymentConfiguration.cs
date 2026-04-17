using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EcommerceDemoV1.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PayOsOrderId)
            .HasMaxLength(100);

        builder.Property(p => p.PayOsTransactionId)
            .HasMaxLength(100);

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(p => p.Method)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne(p => p.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}