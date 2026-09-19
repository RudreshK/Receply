using Receply.Domain.Common;

namespace Receply.Domain.Scheduling;

/// <summary>An offering a tenant sells (e.g. "Haircut", "Oil Change", "Dental Checkup").</summary>
public class Service : TenantOwnedEntity
{
    public string Name { get; private set; } = default!;
    public TimeSpan Duration { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Service() { }

    public static Service Create(Guid tenantId, string name, TimeSpan duration, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Service name is required.", nameof(name));
        if (duration <= TimeSpan.Zero)
            throw new ArgumentException("Duration must be positive.", nameof(duration));

        var service = new Service
        {
            Name = name,
            Duration = duration,
            Price = price
        };
        service.TenantId = tenantId;
        return service;
    }

    public void Deactivate() => IsActive = false;
}
