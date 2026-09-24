using Receply.Domain.Common;

namespace Receply.Domain.Crm;

public class Client : TenantOwnedEntity
{
    public string? FullName { get; private set; }
    public string PhoneNumber { get; private set; } = default!;
    public string? Email { get; private set; }

    private Client() { }

    public static Client Create(Guid tenantId, string phoneNumber, string? fullName = null)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

        var client = new Client
        {
            PhoneNumber = phoneNumber,
            FullName = fullName
        };
        client.TenantId = tenantId;
        return client;
    }

    public void UpdateProfile(string? fullName, string? email)
    {
        FullName = fullName ?? FullName;
        Email = email ?? Email;
    }
}
