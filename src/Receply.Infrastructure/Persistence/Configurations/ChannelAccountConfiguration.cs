using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Channels;

namespace Receply.Infrastructure.Persistence.Configurations;

public class ChannelAccountConfiguration : IEntityTypeConfiguration<ChannelAccount>
{
    public void Configure(EntityTypeBuilder<ChannelAccount> builder)
    {
        builder.ToTable("channel_accounts");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.ExternalId).IsRequired().HasMaxLength(200);
        builder.Property(c => c.DisplayName).IsRequired().HasMaxLength(200);
        builder.HasIndex(c => new { c.Type, c.ExternalId }).IsUnique();
    }
}
