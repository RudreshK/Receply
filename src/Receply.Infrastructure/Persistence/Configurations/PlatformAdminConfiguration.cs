using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Platform;

namespace Receply.Infrastructure.Persistence.Configurations;

public class PlatformAdminConfiguration : IEntityTypeConfiguration<PlatformAdmin>
{
    public void Configure(EntityTypeBuilder<PlatformAdmin> builder)
    {
        builder.ToTable("PlatformAdmin");
        builder.HasKey(p => p.Id).HasName("PK_PlatformAdmin");
        builder.Property(p => p.Id).HasColumnName("PlatformAdminId");
        builder.Property(p => p.FullName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Email).IsRequired().HasMaxLength(320);
        builder.Property(p => p.PhoneNumber).IsRequired().HasMaxLength(32);

        builder.HasIndex(p => p.PhoneNumber).IsUnique().HasDatabaseName("UQ_PlatformAdmin_PhoneNumber");
    }
}
