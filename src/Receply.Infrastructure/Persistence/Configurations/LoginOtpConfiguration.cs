using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Tenancy;

namespace Receply.Infrastructure.Persistence.Configurations;

public class LoginOtpConfiguration : IEntityTypeConfiguration<LoginOtp>
{
    public void Configure(EntityTypeBuilder<LoginOtp> builder)
    {
        builder.ToTable("LoginOtp");
        builder.HasKey(o => o.Id).HasName("PK_LoginOtp");
        builder.Property(o => o.Id).HasColumnName("LoginOtpId");
        builder.Property(o => o.CodeHash).IsRequired().HasMaxLength(128);

        builder.HasOne<Staff>().WithMany().HasForeignKey(o => o.StaffId)
            .HasConstraintName("FK_LoginOtp_Staff").OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(o => o.StaffId).HasDatabaseName("IX_LoginOtp_StaffId");
    }
}
