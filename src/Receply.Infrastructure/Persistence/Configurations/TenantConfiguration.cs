using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Tenancy;

namespace Receply.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenant");
        builder.HasKey(t => t.Id).HasName("PK_Tenant");
        builder.Property(t => t.Id).HasColumnName("TenantId");
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.TimeZoneId).IsRequired().HasMaxLength(100);
        builder.Property(t => t.BusinessEmail).HasMaxLength(120);
        builder.Property(t => t.Website).HasMaxLength(200);
        builder.Property(t => t.OwnerName).HasMaxLength(60);
        builder.Property(t => t.Designation).HasMaxLength(60);
        // Logo/owner photo are stored as data: URLs (no blob storage provisioned yet) - left unbounded
        // (maps to Postgres "text") since a resized image can run a few MB of base64.
        builder.Property(t => t.LogoDataUrl);
        builder.Property(t => t.OwnerPhotoDataUrl);

        builder.HasMany(t => t.Branches)
            .WithOne()
            .HasForeignKey(b => b.TenantId)
            .HasConstraintName("FK_Branch_Tenant")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
