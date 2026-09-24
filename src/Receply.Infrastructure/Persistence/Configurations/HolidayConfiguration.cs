using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Tenancy;

namespace Receply.Infrastructure.Persistence.Configurations;

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("Holiday");
        builder.HasKey(h => h.Id).HasName("PK_Holiday");
        builder.Property(h => h.Id).HasColumnName("HolidayId");
        builder.Property(h => h.Name).IsRequired().HasMaxLength(200);

        builder.HasOne<Branch>().WithMany().HasForeignKey(h => h.BranchId)
            .HasConstraintName("FK_Holiday_Branch").OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(h => new { h.TenantId, h.Date }).HasDatabaseName("IX_Holiday_Tenant_Date");
    }
}
