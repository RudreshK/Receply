using Receply.Domain.Common;

namespace Receply.Domain.Tenancy;

public enum StaffRole
{
    Owner,
    Admin,
    FrontDesk,
    Provider
}

public class StaffMember : TenantOwnedEntity
{
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public StaffRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;

    private StaffMember() { }

    public static StaffMember Create(Guid tenantId, string fullName, string email, StaffRole role)
    {
        var staff = new StaffMember
        {
            FullName = fullName,
            Email = email,
            Role = role
        };
        staff.TenantId = tenantId;
        return staff;
    }

    public void Deactivate() => IsActive = false;
}
