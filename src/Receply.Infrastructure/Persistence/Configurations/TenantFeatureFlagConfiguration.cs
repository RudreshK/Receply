using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Tenancy;

namespace Receply.Infrastructure.Persistence.Configurations;

public class TenantFeatureFlagConfiguration : IEntityTypeConfiguration<TenantFeatureFlag>
{
    public void Configure(EntityTypeBuilder<TenantFeatureFlag> builder)
    {
        builder.ToTable("TenantFeatureFlag");
        builder.HasKey(f => f.Id).HasName("PK_TenantFeatureFlag");
        builder.Property(f => f.Id).HasColumnName("TenantFeatureFlagId");
        builder.HasIndex(f => new { f.TenantId, f.Feature }).IsUnique().HasDatabaseName("UQ_TenantFeatureFlag_TenantId_Feature");
    }
}
