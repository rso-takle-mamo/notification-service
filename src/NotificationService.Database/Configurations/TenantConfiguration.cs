using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Database.Entities;

namespace NotificationService.Database.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(t => t.BusinessName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.BusinessEmail)
            .HasMaxLength(255);

        builder.Property(t => t.BusinessPhone)
            .HasMaxLength(100);

        builder.Property(t => t.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate();

        // Indexes
        builder.HasIndex(t => t.BusinessName);
    }
}