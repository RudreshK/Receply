using Receply.Domain.Common;

namespace Receply.Domain.Tenancy;

// New values are appended, never inserted, since the enum is persisted as a plain int - reordering
// would silently reinterpret every already-stored role.
public enum StaffRole
{
    Owner,
    Admin,
    FrontDesk,
    Provider,
    BranchOwner
}

public class Staff : TenantOwnedEntity
{
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public StaffRole Role { get; private set; }

    // Null only for Owner - the tenant-wide account tied to the main business number. Every other
    // role is confined to one branch; a BranchOwner can only manage staff within their own BranchId.
    public Guid? BranchId { get; private set; }
    public string? Designation { get; private set; }
    public int ExperienceYears { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Staff() { }

    public static Staff Create(
        Guid tenantId, string fullName, string email, string phoneNumber, StaffRole role,
        Guid? branchId, string? designation = null, int experienceYears = 0)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));
        if (role != StaffRole.Owner && branchId is null)
            throw new ArgumentException("Branch is required for every role except Owner.", nameof(branchId));
        if (experienceYears < 0)
            throw new ArgumentException("Experience years cannot be negative.", nameof(experienceYears));

        var staff = new Staff
        {
            FullName = fullName,
            Email = email,
            PhoneNumber = phoneNumber,
            Role = role,
            BranchId = role == StaffRole.Owner ? null : branchId,
            Designation = designation,
            ExperienceYears = experienceYears
        };
        staff.TenantId = tenantId;
        return staff;
    }

    public void UpdateProfile(string fullName, string email, string? designation, int experienceYears)
    {
        if (experienceYears < 0)
            throw new ArgumentException("Experience years cannot be negative.", nameof(experienceYears));

        FullName = fullName;
        Email = email;
        Designation = designation;
        ExperienceYears = experienceYears;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
