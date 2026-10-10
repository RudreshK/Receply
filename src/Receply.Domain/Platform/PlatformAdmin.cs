using Receply.Domain.Common;

namespace Receply.Domain.Platform;

/// <summary>
/// The SaaS product owner's own account - not tied to any tenant, and not a Staff row (Staff is
/// always tenant-owned). Logs in the same OTP-by-phone way as a business's staff, but the JWT it
/// gets carries no tenant_id claim, so the ambient tenant query filter stays a no-op and every
/// tenant's data is visible. There is typically exactly one of these.
/// </summary>
public class PlatformAdmin : AggregateRoot
{
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private PlatformAdmin() { }

    public static PlatformAdmin Create(string fullName, string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

        return new PlatformAdmin
        {
            FullName = fullName,
            Email = email,
            PhoneNumber = phoneNumber
        };
    }

    public void Deactivate() => IsActive = false;
}
