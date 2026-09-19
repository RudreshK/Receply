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
        builder.HasIndex(m => new { m.ConversationId, m.SentAtUtc }).HasDatabaseName("IX_Message_ConversationId_SentAtUtc");

        // Messages are immutable, high-volume, and purely transactional - a sent/received message
        // is never edited - so the "modified" and concurrency-versioning parts of the audit
        // baseline don't apply here. CreatedOn/CreatedBy/IsDeleted still do.
        builder.Ignore(m => m.ModifiedOn);
        builder.Ignore(m => m.ModifiedBy);
        builder.Ignore(m => m.RowVersion);
    }
}
