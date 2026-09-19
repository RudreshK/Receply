using Receply.Domain.Common;

namespace Receply.Domain.Crm;

public class Customer : TenantOwnedEntity
{
    public string? FullName { get; private set; }
    public string PhoneNumber { get; private set; } = default!;
    public string? Email { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    private Customer() { }

    public static Customer Create(Guid tenantId, string phoneNumber, string? fullName = null)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

        var customer = new Customer
        {
            PhoneNumber = phoneNumber,
            FullName = fullName
        };
        customer.TenantId = tenantId;
        return customer;
    }

    public void UpdateProfile(string? fullName, string? email)
    {
        FullName = fullName ?? FullName;
        Email = email ?? Email;
    }
}
