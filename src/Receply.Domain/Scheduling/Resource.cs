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
    public Guid BranchId { get; private set; }
    public string Name { get; private set; } = default!;
    public ResourceType Type { get; private set; }

    // Set when Type is Staff and this resource IS a real staff account's bookable calendar - lets
    // availability/booking subtract that staff member's StaffLeave days from this resource's slots.
    // Null for Room/Bay/Equipment resources, which have no staff member to go on leave.
    public Guid? StaffId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<AvailabilityRule> _availabilityRules = [];
    public IReadOnlyCollection<AvailabilityRule> AvailabilityRules => _availabilityRules.AsReadOnly();

    private Resource() { }

    public static Resource Create(Guid tenantId, Guid branchId, string name, ResourceType type, Guid? staffId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Resource name is required.", nameof(name));

        var resource = new Resource
        {
            BranchId = branchId,
            Name = name,
            Type = type,
            StaffId = staffId
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

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
