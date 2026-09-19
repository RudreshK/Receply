using Receply.Domain.Common;

namespace Receply.Domain.Scheduling;

/// <summary>Recurring weekly working hours for a resource (e.g. "Mon 09:00-17:00").</summary>
public class AvailabilityRule : TenantOwnedEntity
{
    public Guid ResourceId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    private AvailabilityRule() { }

    internal static AvailabilityRule Create(Guid tenantId, Guid resourceId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time.", nameof(endTime));

        var rule = new AvailabilityRule
        {
            ResourceId = resourceId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime
        };
        rule.TenantId = tenantId;
        return rule;
    }
}
