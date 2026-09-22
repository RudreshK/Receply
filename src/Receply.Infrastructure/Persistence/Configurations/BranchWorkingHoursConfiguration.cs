using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Tenancy;

namespace Receply.Infrastructure.Persistence.Configurations;

public class BranchWorkingHoursConfiguration : IEntityTypeConfiguration<BranchWorkingHours>
{
    public void Configure(EntityTypeBuilder<BranchWorkingHours> builder)
    {
        builder.ToTable("BranchWorkingHours");
        builder.HasKey(h => h.Id).HasName("PK_BranchWorkingHours");
        builder.Property(h => h.Id).HasColumnName("BranchWorkingHoursId");

        builder.HasOne<Branch>().WithMany().HasForeignKey(h => h.BranchId)
            .HasConstraintName("FK_BranchWorkingHours_Branch").OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(h => new { h.BranchId, h.DayOfWeek }).IsUnique().HasDatabaseName("UQ_BranchWorkingHours_Branch_DayOfWeek");
    }
}
