using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Crm;

namespace Receply.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Client");
        builder.HasKey(c => c.Id).HasName("PK_Client");
        builder.Property(c => c.Id).HasColumnName("ClientId");
        builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(32);
        builder.Property(c => c.FullName).HasMaxLength(200);
        builder.Property(c => c.Email).HasMaxLength(320);
        builder.HasIndex(c => new { c.TenantId, c.PhoneNumber }).IsUnique().HasDatabaseName("UQ_Client_Tenant_PhoneNumber");
    }
}
