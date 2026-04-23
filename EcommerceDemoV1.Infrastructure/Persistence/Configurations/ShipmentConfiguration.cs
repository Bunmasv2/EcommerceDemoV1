using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EcommerceDemoV1.Domain.Entities;

namespace EcommerceDemoV1.Infrastructure.Persistence.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.AhamoveOrderId)
            .HasMaxLength(100);

        builder.Property(s => s.ServiceId)
            .HasMaxLength(50);

        // AhaMove Statuses: IDLE, ASSIGNING, ACCEPTED, IN_PROCESS, COMPLETED, CANCELLED
        builder.Property(s => s.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.TrackingUrl)
            .HasMaxLength(500);

        builder.Property(s => s.ShippingFee)
            .HasColumnType("decimal(18,0)");

        builder.Property(s => s.DriverName)
            .HasMaxLength(100);

        builder.Property(s => s.DriverPhone)
            .HasMaxLength(20);

        builder.HasOne(s => s.Order)
            .WithMany(o => o.Shipments)
            .HasForeignKey(s => s.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}