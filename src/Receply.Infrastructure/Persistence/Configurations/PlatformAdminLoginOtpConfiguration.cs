using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Platform;

namespace Receply.Infrastructure.Persistence.Configurations;

public class PlatformAdminLoginOtpConfiguration : IEntityTypeConfiguration<PlatformAdminLoginOtp>
{
    public void Configure(EntityTypeBuilder<PlatformAdminLoginOtp> builder)
    {
        builder.ToTable("PlatformAdminLoginOtp");
        builder.HasKey(o => o.Id).HasName("PK_PlatformAdminLoginOtp");
        builder.Property(o => o.Id).HasColumnName("PlatformAdminLoginOtpId");
        builder.Property(o => o.CodeHash).IsRequired().HasMaxLength(128);

        builder.HasOne<PlatformAdmin>().WithMany().HasForeignKey(o => o.PlatformAdminId)
            .HasConstraintName("FK_PlatformAdminLoginOtp_PlatformAdmin").OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(o => o.PlatformAdminId).HasDatabaseName("IX_PlatformAdminLoginOtp_PlatformAdminId");
    }
}
