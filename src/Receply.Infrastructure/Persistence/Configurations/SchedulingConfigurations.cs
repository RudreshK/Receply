using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Scheduling;

namespace Receply.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Service");
        builder.HasKey(s => s.Id).HasName("PK_Service");
        builder.Property(s => s.Id).HasColumnName("ServiceId");
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Price).HasColumnType("numeric(10,2)");
        builder.HasIndex(s => s.TenantId).HasDatabaseName("IX_Service_TenantId");
    }
}

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("Resource");
        builder.HasKey(r => r.Id).HasName("PK_Resource");
        builder.Property(r => r.Id).HasColumnName("ResourceId");
        builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(r => new { r.TenantId, r.BranchId }).HasDatabaseName("IX_Resource_TenantId_BranchId");

        builder.HasMany(r => r.AvailabilityRules)
            .WithOne()
            .HasForeignKey(a => a.ResourceId)
            .HasConstraintName("FK_AvailabilityRule_Resource")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AvailabilityRuleConfiguration : IEntityTypeConfiguration<AvailabilityRule>
{
    public void Configure(EntityTypeBuilder<AvailabilityRule> builder)
    {
        builder.ToTable("AvailabilityRule");
        builder.HasKey(a => a.Id).HasName("PK_AvailabilityRule");
        builder.Property(a => a.Id).HasColumnName("AvailabilityRuleId");
        builder.HasIndex(a => new { a.ResourceId, a.DayOfWeek }).HasDatabaseName("IX_AvailabilityRule_ResourceId_DayOfWeek");
    }
}

public class TimeBlockConfiguration : IEntityTypeConfiguration<TimeBlock>
{
    public void Configure(EntityTypeBuilder<TimeBlock> builder)
    {
        builder.ToTable("TimeBlock");
        builder.HasKey(t => t.Id).HasName("PK_TimeBlock");
        builder.Property(t => t.Id).HasColumnName("TimeBlockId");
        builder.Property(t => t.Reason).HasMaxLength(500);
        builder.HasIndex(t => new { t.ResourceId, t.StartUtc, t.EndUtc }).HasDatabaseName("IX_TimeBlock_ResourceId_StartUtc_EndUtc");
    }
}

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointment");
        builder.HasKey(a => a.Id).HasName("PK_Appointment");
        builder.Property(a => a.Id).HasColumnName("AppointmentId");
        builder.Property(a => a.CancellationReason).HasMaxLength(500);
        builder.HasIndex(a => new { a.ResourceId, a.StartUtc, a.EndUtc }).HasDatabaseName("IX_Appointment_ResourceId_StartUtc_EndUtc");
        builder.HasIndex(a => new { a.TenantId, a.ClientId }).HasDatabaseName("IX_Appointment_TenantId_ClientId");
    }
}
