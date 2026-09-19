using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Scheduling;

namespace Receply.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Price).HasColumnType("numeric(10,2)");
        builder.HasIndex(s => s.TenantId);
    }
}

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("resources");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(r => new { r.TenantId, r.LocationId });

        builder.HasMany(r => r.AvailabilityRules)
            .WithOne()
            .HasForeignKey(a => a.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AvailabilityRuleConfiguration : IEntityTypeConfiguration<AvailabilityRule>
{
    public void Configure(EntityTypeBuilder<AvailabilityRule> builder)
    {
        builder.ToTable("availability_rules");
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => new { a.ResourceId, a.DayOfWeek });
    }
}

public class TimeBlockConfiguration : IEntityTypeConfiguration<TimeBlock>
{
    public void Configure(EntityTypeBuilder<TimeBlock> builder)
    {
        builder.ToTable("time_blocks");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Reason).HasMaxLength(500);
        builder.HasIndex(t => new { t.ResourceId, t.StartUtc, t.EndUtc });
    }
}

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.CancellationReason).HasMaxLength(500);
        builder.HasIndex(a => new { a.ResourceId, a.StartUtc, a.EndUtc });
        builder.HasIndex(a => new { a.TenantId, a.CustomerId });

        // Postgres system column used as an optimistic concurrency token, so two
        // simultaneous booking requests for the same slot can't both succeed silently.
        builder.Property<uint>("xmin")
            .IsRowVersion()
            .HasColumnName("xmin");
    }
}
