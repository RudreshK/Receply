using Receply.Domain.Common;

namespace Receply.Domain.Tenancy;

/// <summary>A day the business is closed. BranchId null means it applies to every branch.</summary>
public class Holiday : TenantOwnedEntity
{
    public Guid? BranchId { get; private set; }
    public DateOnly Date { get; private set; }
    public string Name { get; private set; } = default!;

    private Holiday() { }

    public static Holiday Create(Guid tenantId, Guid? branchId, DateOnly date, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Holiday name is required.", nameof(name));

        var holiday = new Holiday
        {
            BranchId = branchId,
            Date = date,
            Name = name
        };
        holiday.TenantId = tenantId;
        return holiday;
    }
}
