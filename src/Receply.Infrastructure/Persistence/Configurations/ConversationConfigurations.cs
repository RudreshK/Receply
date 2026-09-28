using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Receply.Domain.Conversations;

namespace Receply.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversation");
        builder.HasKey(c => c.Id).HasName("PK_Conversation");
        builder.Property(c => c.Id).HasColumnName("ConversationId");
        builder.HasIndex(c => new { c.TenantId, c.Status }).HasDatabaseName("IX_Conversation_TenantId_Status");
        builder.HasIndex(c => c.ClientId).HasDatabaseName("IX_Conversation_ClientId");

        builder.HasMany(c => c.Messages)
            .WithOne()
            .HasForeignKey(m => m.ConversationId)
            .HasConstraintName("FK_Message_Conversation")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Message");
        builder.HasKey(m => m.Id).HasName("PK_Message");
        builder.Property(m => m.Id).HasColumnName("MessageId");
        builder.Property(m => m.Body).IsRequired();
        builder.Property(m => m.ExternalMessageId).HasMaxLength(128);
        builder.HasIndex(m => new { m.ConversationId, m.SentAtUtc }).HasDatabaseName("IX_Message_ConversationId_SentAtUtc");
        builder.HasIndex(m => m.ExternalMessageId).HasDatabaseName("IX_Message_ExternalMessageId");

        // A message's body/sender/etc. are immutable once sent or received - CreatedOn/CreatedBy/
        // IsDeleted still apply from the audit baseline, but ModifiedOn/ModifiedBy/RowVersion don't,
        // even though DeliveryStatus itself does get updated in place as WhatsApp status webhooks
        // (sent/delivered/read) arrive - that's a delivery-tracking field, not an edit to the message.
        builder.Ignore(m => m.ModifiedOn);
        builder.Ignore(m => m.ModifiedBy);
        builder.Ignore(m => m.RowVersion);
    }
}
