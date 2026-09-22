using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Tenancy;

namespace Receply.Infrastructure.Persistence.Configurations;

public class SignupOtpConfiguration : IEntityTypeConfiguration<SignupOtp>
{
    public void Configure(EntityTypeBuilder<SignupOtp> builder)
    {
        builder.ToTable("SignupOtp");
        builder.HasKey(o => o.Id).HasName("PK_SignupOtp");
        builder.Property(o => o.Id).HasColumnName("SignupOtpId");
        builder.Property(o => o.PhoneNumber).IsRequired().HasMaxLength(32);
        builder.Property(o => o.CodeHash).IsRequired().HasMaxLength(128);
        builder.HasIndex(o => o.PhoneNumber).HasDatabaseName("IX_SignupOtp_PhoneNumber");
    }
}
