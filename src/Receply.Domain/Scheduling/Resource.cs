using Receply.Domain.Common;

namespace Receply.Domain.Scheduling;

public enum ResourceType
{
    Staff,
    Room,
    Bay,
    Equipment
}

/// <summary>Whatever an appointment is booked against: a stylist, a doctor, a service bay, a massage room...</summary>
public class Resource : TenantOwnedEntity
{
    public Guid LocationId { get; private set; }
    public string Name { get; private set; } = default!;
    public ResourceType Type { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<AvailabilityRule> _availabilityRules = [];
    public IReadOnlyCollection<AvailabilityRule> AvailabilityRules => _availabilityRules.AsReadOnly();

    private Resource() { }

    public static Resource Create(Guid tenantId, Guid locationId, string name, ResourceType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Resource name is required.", nameof(name));

        var resource = new Resource
        {
            LocationId = locationId,
            Name = name,
            Type = type
        };
        resource.TenantId = tenantId;
        return resource;
    }

    public AvailabilityRule AddAvailabilityRule(DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        var rule = AvailabilityRule.Create(TenantId, Id, dayOfWeek, startTime, endTime);
        _availabilityRules.Add(rule);
        return rule;
    }

    public void Deactivate() => IsActive = false;
}
