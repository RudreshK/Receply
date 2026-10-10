using Receply.Domain.Common;

namespace Receply.Domain.Tenancy;

/// <summary>
/// A day (or part of a day) one specific staff member is off, distinct from Holiday (which closes
/// a whole branch/tenant). StartTime/EndTime null means the whole day; set means a half-day leave
/// (e.g. 09:00-13:00), so booking capacity for the rest of the day is unaffected.
/// </summary>
public class StaffLeave : TenantOwnedEntity
{
    public Guid StaffId { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly? StartTime { get; private set; }
    public TimeOnly? EndTime { get; private set; }
    public string? Reason { get; private set; }

    private StaffLeave() { }

    public static StaffLeave Create(Guid tenantId, Guid staffId, DateOnly date, TimeOnly? startTime, TimeOnly? endTime, string? reason)
    {
        if (startTime.HasValue != endTime.HasValue)
            throw new ArgumentException("Start and end time must both be set, or both left unset for a full-day leave.");
        if (startTime.HasValue && endTime.HasValue && endTime <= startTime)
            throw new ArgumentException("End time must be after start time.", nameof(endTime));

        var leave = new StaffLeave
        {
            StaffId = staffId,
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            Reason = reason
        };
        leave.TenantId = tenantId;
        return leave;
    }

    /// <summary>Whether this leave covers the given local time-of-day window on its Date.</summary>
    public bool Covers(TimeOnly windowStart, TimeOnly windowEnd) =>
        !StartTime.HasValue || (StartTime.Value < windowEnd && EndTime!.Value > windowStart);
}
