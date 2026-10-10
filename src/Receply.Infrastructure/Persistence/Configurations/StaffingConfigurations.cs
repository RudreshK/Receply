using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Scheduling;
using Receply.Domain.Tenancy;

namespace Receply.Infrastructure.Persistence.Configurations;

public class StaffServiceConfiguration : IEntityTypeConfiguration<StaffService>
{
    public void Configure(EntityTypeBuilder<StaffService> builder)
    {
        builder.ToTable("StaffService");
        builder.HasKey(ss => ss.Id).HasName("PK_StaffService");
        builder.Property(ss => ss.Id).HasColumnName("StaffServiceId");

        builder.HasIndex(ss => new { ss.StaffId, ss.ServiceId }).IsUnique().HasDatabaseName("UQ_StaffService_Staff_Service");

        builder.HasOne<Staff>().WithMany().HasForeignKey(ss => ss.StaffId)
            .HasConstraintName("FK_StaffService_Staff").OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Service>().WithMany().HasForeignKey(ss => ss.ServiceId)
            .HasConstraintName("FK_StaffService_Service").OnDelete(DeleteBehavior.Cascade);
    }
}

public class StaffLeaveConfiguration : IEntityTypeConfiguration<StaffLeave>
{
    public void Configure(EntityTypeBuilder<StaffLeave> builder)
    {
        builder.ToTable("StaffLeave");
        builder.HasKey(l => l.Id).HasName("PK_StaffLeave");
        builder.Property(l => l.Id).HasColumnName("StaffLeaveId");
        builder.Property(l => l.Reason).HasMaxLength(500);

        builder.HasIndex(l => new { l.StaffId, l.Date }).IsUnique().HasDatabaseName("UQ_StaffLeave_Staff_Date");

        builder.HasOne<Staff>().WithMany().HasForeignKey(l => l.StaffId)
            .HasConstraintName("FK_StaffLeave_Staff").OnDelete(DeleteBehavior.Cascade);
    }
}
