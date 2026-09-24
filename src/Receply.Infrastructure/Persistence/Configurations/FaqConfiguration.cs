using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Tenancy;

namespace Receply.Infrastructure.Persistence.Configurations;

public class FaqConfiguration : IEntityTypeConfiguration<Faq>
{
    public void Configure(EntityTypeBuilder<Faq> builder)
    {
        builder.ToTable("Faq");
        builder.HasKey(f => f.Id).HasName("PK_Faq");
        builder.Property(f => f.Id).HasColumnName("FaqId");
        builder.Property(f => f.Question).IsRequired().HasMaxLength(500);
        builder.Property(f => f.Answer).IsRequired().HasMaxLength(2000);
        builder.HasIndex(f => new { f.TenantId, f.IsActive }).HasDatabaseName("IX_Faq_Tenant_IsActive");
    }
}
