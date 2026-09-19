using MediatR;

namespace Receply.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTimeOffset CreatedOn { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTimeOffset? ModifiedOn { get; private set; }
    public Guid? ModifiedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public uint RowVersion { get; private set; }

    private readonly List<INotification> _domainEvents = [];
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    protected void Raise(INotification domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>Soft-delete: technical/audit concern, not a business state - distinct from any domain-specific IsActive flag an entity may also have.</summary>
    public void Delete() => IsDeleted = true;
}

public abstract class AggregateRoot : Entity;

public abstract class TenantOwnedEntity : AggregateRoot
{
    public Guid TenantId { get; protected set; }
}
