using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Channels;

namespace Receply.Infrastructure.Persistence.Configurations;

public class ChannelAccountConfiguration : IEntityTypeConfiguration<ChannelAccount>
{
    public void Configure(EntityTypeBuilder<ChannelAccount> builder)
    {
        builder.ToTable("ChannelAccount");
        builder.HasKey(c => c.Id).HasName("PK_ChannelAccount");
        builder.Property(c => c.Id).HasColumnName("ChannelAccountId");
        builder.Property(c => c.ExternalId).IsRequired().HasMaxLength(200);
        builder.Property(c => c.DisplayName).IsRequired().HasMaxLength(200);
        builder.HasIndex(c => new { c.Type, c.ExternalId }).IsUnique().HasDatabaseName("UQ_ChannelAccount_Type_ExternalId");
    }
}
