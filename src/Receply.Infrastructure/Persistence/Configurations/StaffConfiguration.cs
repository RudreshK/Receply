using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Tenancy;

namespace Receply.Infrastructure.Persistence.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("Staff");
        builder.HasKey(s => s.Id).HasName("PK_Staff");
        builder.Property(s => s.Id).HasColumnName("StaffId");
        builder.Property(s => s.FullName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Email).IsRequired().HasMaxLength(320);
        builder.Property(s => s.PhoneNumber).IsRequired().HasMaxLength(32);
        builder.Property(s => s.Designation).HasMaxLength(200);
        builder.HasIndex(s => new { s.TenantId, s.Email }).IsUnique().HasDatabaseName("UQ_Staff_Tenant_Email");

        // Global (not tenant-scoped) - OTP login looks a staff member up by phone before any tenant is known.
        builder.HasIndex(s => s.PhoneNumber).IsUnique().HasDatabaseName("UQ_Staff_PhoneNumber");

        // Null for Owner (tenant-wide); Restrict so a branch can't be deleted out from under staff
        // still assigned to it.
        builder.HasOne<Branch>().WithMany().HasForeignKey(s => s.BranchId)
            .HasConstraintName("FK_Staff_Branch").OnDelete(DeleteBehavior.Restrict);
    }
}
