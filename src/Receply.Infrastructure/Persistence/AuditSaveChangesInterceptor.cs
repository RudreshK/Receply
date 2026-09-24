using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Receply.Domain.Common;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Infrastructure.Persistence;

/// <summary>
/// Stamps CreatedOn/CreatedBy on inserts and ModifiedOn/ModifiedBy on updates for every Entity.
/// CreatedBy/ModifiedBy are null when there's no authenticated staff actor (webhook/background-queue
/// paths) - that correctly represents a system/AI-originated change, not a bug.
/// </summary>
public class AuditSaveChangesInterceptor(ICurrentUserContext currentUserContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
            return;

        var now = DateTimeOffset.UtcNow;
        var staffId = currentUserContext.StaffId;

        foreach (var entry in context.ChangeTracker.Entries<Entity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    SetIfMapped(entry, nameof(Entity.CreatedOn), now);
                    SetIfMapped(entry, nameof(Entity.CreatedBy), staffId);
                    break;
                case EntityState.Modified:
                    SetIfMapped(entry, nameof(Entity.ModifiedOn), now);
                    SetIfMapped(entry, nameof(Entity.ModifiedBy), staffId);
                    break;
            }
        }
    }

    /// <summary>Some entities (e.g. Message) exclude ModifiedOn/ModifiedBy from mapping entirely - skip those rather than throw.</summary>
    private static void SetIfMapped(EntityEntry entry, string propertyName, object? value)
    {
        if (entry.Metadata.FindProperty(propertyName) is not null)
            entry.Property(propertyName).CurrentValue = value;
    }
}
