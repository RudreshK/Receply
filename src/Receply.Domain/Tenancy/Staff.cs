using Receply.Domain.Common;

namespace Receply.Domain.Tenancy;

public enum StaffRole
{
    Owner,
    Admin,
    FrontDesk,
    Provider
}

public class Staff : TenantOwnedEntity
{
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public StaffRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;

    /// <summary>ISO 639-1 code (e.g. "te", "hi") for the dashboard's "Translate" feature - null means
    /// no preference set, and the dashboard defaults to English.</summary>
    public string? PreferredLanguage { get; private set; }

    private Staff() { }

    public static Staff Create(Guid tenantId, string fullName, string email, string phoneNumber, StaffRole role)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

        var staff = new Staff
        {
            FullName = fullName,
            Email = email,
            PhoneNumber = phoneNumber,
            Role = role
        };
        staff.TenantId = tenantId;
        return staff;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void UpdateOwnDetails(string fullName, string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

        FullName = fullName;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public void SetPreferredLanguage(string? languageCode) => PreferredLanguage = languageCode;
}
