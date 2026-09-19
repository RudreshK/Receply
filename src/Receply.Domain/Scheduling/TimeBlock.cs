using Receply.Domain.Common;

namespace Receply.Domain.Scheduling;

/// <summary>A one-off block on a resource's calendar (holiday, sick day, maintenance) that overrides AvailabilityRule.</summary>
public class TimeBlock : TenantOwnedEntity
{
    public Guid ResourceId { get; private set; }
    public DateTimeOffset StartUtc { get; private set; }
    public DateTimeOffset EndUtc { get; private set; }
    public string? Reason { get; private set; }

    private TimeBlock() { }

    public static TimeBlock Create(Guid tenantId, Guid resourceId, DateTimeOffset startUtc, DateTimeOffset endUtc, string? reason = null)
    {
        if (endUtc <= startUtc)
            throw new ArgumentException("End must be after start.", nameof(endUtc));

        var block = new TimeBlock
        {
            ResourceId = resourceId,
            StartUtc = startUtc,
            EndUtc = endUtc,
            Reason = reason
        };
        block.TenantId = tenantId;
        return block;
    }
}
