using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Platform;

namespace Receply.Infrastructure.Persistence.Configurations;

public class BusinessEnquiryConfiguration : IEntityTypeConfiguration<BusinessEnquiry>
{
    public void Configure(EntityTypeBuilder<BusinessEnquiry> builder)
    {
        builder.ToTable("BusinessEnquiry");
        builder.HasKey(e => e.Id).HasName("PK_BusinessEnquiry");
        builder.Property(e => e.Id).HasColumnName("BusinessEnquiryId");
        builder.Property(e => e.BusinessName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.ContactName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(32);
        builder.Property(e => e.Email).HasMaxLength(320);
        builder.Property(e => e.Notes).HasMaxLength(2000);
    }
}
