using Receply.Domain.Common;

namespace Receply.Domain.Tenancy;

/// <summary>Business-level operating hours for a branch (distinct from a Resource's own AvailabilityRule calendar).</summary>
public class BranchWorkingHours : TenantOwnedEntity
{
    public Guid BranchId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly? OpenTime { get; private set; }
    public TimeOnly? CloseTime { get; private set; }
    public bool IsClosed { get; private set; }

    private BranchWorkingHours() { }

    public static BranchWorkingHours Create(Guid tenantId, Guid branchId, DayOfWeek dayOfWeek, TimeOnly? openTime, TimeOnly? closeTime, bool isClosed)
    {
        if (!isClosed && (openTime is null || closeTime is null || closeTime <= openTime))
            throw new ArgumentException("Open and close times are required and close must be after open when the branch isn't closed that day.");

        var hours = new BranchWorkingHours
        {
            BranchId = branchId,
            DayOfWeek = dayOfWeek,
            OpenTime = isClosed ? null : openTime,
            CloseTime = isClosed ? null : closeTime,
            IsClosed = isClosed
        };
        hours.TenantId = tenantId;
        return hours;
    }
}
