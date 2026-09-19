using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Tenancy;

namespace Receply.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.TimeZoneId).IsRequired().HasMaxLength(100);

        builder.HasMany(t => t.Locations)
            .WithOne()
            .HasForeignKey(l => l.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Name).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Address).IsRequired().HasMaxLength(500);
        builder.HasIndex(l => l.TenantId);
    }
}

public class StaffMemberConfiguration : IEntityTypeConfiguration<StaffMember>
{
    public void Configure(EntityTypeBuilder<StaffMember> builder)
    {
        builder.ToTable("staff_members");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.FullName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Email).IsRequired().HasMaxLength(320);
        builder.HasIndex(s => new { s.TenantId, s.Email }).IsUnique();
    }
}
