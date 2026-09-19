using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Tenancy;

namespace Receply.Infrastructure.Persistence.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branch");
        builder.HasKey(b => b.Id).HasName("PK_Branch");
        builder.Property(b => b.Id).HasColumnName("BranchId");
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Address).IsRequired().HasMaxLength(500);
        builder.HasIndex(b => b.TenantId).HasDatabaseName("IX_Branch_TenantId");
    }
}
