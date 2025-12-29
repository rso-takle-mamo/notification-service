using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Database.Entities;

namespace NotificationService.Database.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(b => b.TenantId)
            .IsRequired();

        builder.Property(b => b.StartDateTime)
            .IsRequired();

        builder.Property(b => b.EndDateTime)
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.Property(b => b.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate();

        // Indexes
        builder.HasIndex(b => b.TenantId);
        builder.HasIndex(b => new { b.TenantId, b.StartDateTime });

        // Relationships
        builder.HasOne(b => b.Tenant)
            .WithMany()
            .HasForeignKey(b => b.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}