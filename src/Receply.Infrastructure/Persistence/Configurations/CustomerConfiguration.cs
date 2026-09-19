using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Crm;

namespace Receply.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(32);
        builder.Property(c => c.FullName).HasMaxLength(200);
        builder.Property(c => c.Email).HasMaxLength(320);
        builder.HasIndex(c => new { c.TenantId, c.PhoneNumber }).IsUnique();
    }
}
