using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EcommerceDemoV1.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.SubTotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.DiscountAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.PromotionDiscount)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.RankDiscount)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.CouponDiscount)
        .HasColumnType("decimal(18,2)");

        builder.Property(o => o.ShippingFee)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.FinalTotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(o => o.PaymentStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(o => o.ShippingAddress)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(o => o.TrackingCode)
            .HasMaxLength(100);

        builder.Property(o => o.PayOsOrderId)
            .HasMaxLength(100);

        builder.Property(o => o.CouponCode)
            .HasMaxLength(50);

        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Coupon)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CouponId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}