using Receply.Domain.Common;

namespace Receply.Domain.Tenancy;

public enum TenantStatus
{
    TrialActive,
    Active,
    PastDue,
    Suspended
}

public class Tenant : AggregateRoot
{
    public string Name { get; private set; } = default!;
    public BusinessType BusinessType { get; private set; }
    public string TimeZoneId { get; private set; } = "UTC";
    public TenantStatus Status { get; private set; } = TenantStatus.TrialActive;
    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    private readonly List<Location> _locations = [];
    public IReadOnlyCollection<Location> Locations => _locations.AsReadOnly();

    private Tenant() { }

    public static Tenant Create(string name, BusinessType businessType, string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name is required.", nameof(name));

        return new Tenant
        {
            Name = name,
            BusinessType = businessType,
            TimeZoneId = timeZoneId
        };
    }

    public Location AddLocation(string name, string address)
    {
        var location = Location.Create(Id, name, address);
        _locations.Add(location);
        return location;
    }

    public void Suspend() => Status = TenantStatus.Suspended;

    public void Reactivate() => Status = TenantStatus.Active;
}
