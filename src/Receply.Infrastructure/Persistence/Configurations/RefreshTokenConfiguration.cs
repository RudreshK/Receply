using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Tenancy;

namespace Receply.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshToken");
        builder.HasKey(t => t.Id).HasName("PK_RefreshToken");
        builder.Property(t => t.Id).HasColumnName("RefreshTokenId");
        builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(128);

        builder.HasOne<Staff>().WithMany().HasForeignKey(t => t.StaffId)
            .HasConstraintName("FK_RefreshToken_Staff").OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(t => t.StaffId).HasDatabaseName("IX_RefreshToken_StaffId");

        // Looked up by hash alone (no tenant/staff known yet at refresh time) - must be unique.
        builder.HasIndex(t => t.TokenHash).IsUnique().HasDatabaseName("UQ_RefreshToken_TokenHash");
    }
}
