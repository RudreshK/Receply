using Receply.Domain.Common;

namespace Receply.Domain.Tenancy;

public class Branch : TenantOwnedEntity
{
    public string Name { get; private set; } = default!;
    public string Address { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private Branch() { }

    internal static Branch Create(Guid tenantId, string name, string address)
    {
        var branch = new Branch
        {
            Name = name,
            Address = address
        };
        branch.TenantId = tenantId;
        return branch;
    }

    public void Deactivate() => IsActive = false;
}
