using Receply.Domain.Common;

namespace Receply.Domain.Tenancy;

public class Location : TenantOwnedEntity
{
    public string Name { get; private set; } = default!;
    public string Address { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private Location() { }

    internal static Location Create(Guid tenantId, string name, string address)
    {
        var location = new Location
        {
            Name = name,
            Address = address
        };
        location.TenantId = tenantId;
        return location;
    }

    public void Deactivate() => IsActive = false;
}
