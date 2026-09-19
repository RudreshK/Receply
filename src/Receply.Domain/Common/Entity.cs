using MediatR;

namespace Receply.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    private readonly List<INotification> _domainEvents = [];
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    protected void Raise(INotification domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}

public abstract class AggregateRoot : Entity;

public abstract class TenantOwnedEntity : AggregateRoot
{
    public Guid TenantId { get; protected set; }
}
