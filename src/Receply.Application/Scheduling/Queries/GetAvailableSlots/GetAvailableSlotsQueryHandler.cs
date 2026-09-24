using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Scheduling;

namespace Receply.Application.Scheduling.Queries.GetAvailableSlots;

/// <summary>
/// Finds open slots for a service on a given day. Simplification: there's no Service-Resource
/// capability mapping yet, so this treats every active resource of the tenant as able to perform
/// any service - fine for a single-location, single-specialty business (a typical MVP tenant);
/// a proper "which resources can do which services" mapping is the next iteration for tenants
/// with specialized staff (e.g. only some stylists cut hair vs. color).
/// </summary>
public class GetAvailableSlotsQueryHandler(IApplicationDbContext db) : IRequestHandler<GetAvailableSlotsQuery, IReadOnlyList<AvailableSlotDto>>
{
    private const int MaxSlotsReturned = 12;

    public async Task<IReadOnlyList<AvailableSlotDto>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant '{request.TenantId}' not found.");

        var service = await db.Services.FirstOrDefaultAsync(
            s => s.TenantId == request.TenantId && s.IsActive && s.Name.ToLower() == request.ServiceName.ToLower(),
            cancellationToken) ?? throw new KeyNotFoundException($"Service '{request.ServiceName}' not found.");

        var timeZone = ResolveTimeZone(tenant.TimeZoneId);
        var dayOfWeek = request.Date.DayOfWeek;

        var dayStartUtc = new DateTimeOffset(request.Date.ToDateTime(TimeOnly.MinValue), timeZone.GetUtcOffset(request.Date.ToDateTime(TimeOnly.MinValue)));
        var dayEndUtc = dayStartUtc.AddDays(1);

        var resources = await db.Resources
            .Where(r => r.TenantId == request.TenantId && r.IsActive)
            .ToListAsync(cancellationToken);

        var resourceIds = resources.Select(r => r.Id).ToList();

        var rules = await db.AvailabilityRules
            .Where(a => resourceIds.Contains(a.ResourceId) && a.DayOfWeek == dayOfWeek)
            .ToListAsync(cancellationToken);

        var timeBlocks = await db.TimeBlocks
            .Where(b => resourceIds.Contains(b.ResourceId) && b.StartUtc < dayEndUtc && b.EndUtc > dayStartUtc)
            .ToListAsync(cancellationToken);

        var existingAppointments = await db.Appointments
            .Where(a => resourceIds.Contains(a.ResourceId) && a.Status != AppointmentStatus.Cancelled
                        && a.StartUtc < dayEndUtc && a.EndUtc > dayStartUtc)
            .ToListAsync(cancellationToken);

        var slots = new List<AvailableSlotDto>();

        foreach (var resource in resources)
        {
            foreach (var rule in rules.Where(r => r.ResourceId == resource.Id))
            {
                var cursorLocal = request.Date.ToDateTime(rule.StartTime);
                var ruleEndLocal = request.Date.ToDateTime(rule.EndTime);

                while (cursorLocal + service.Duration <= ruleEndLocal)
                {
                    var slotStartUtc = new DateTimeOffset(cursorLocal, timeZone.GetUtcOffset(cursorLocal));
                    var slotEndUtc = slotStartUtc + service.Duration;

                    var blocked = timeBlocks.Any(b => b.ResourceId == resource.Id && b.StartUtc < slotEndUtc && b.EndUtc > slotStartUtc)
                        || existingAppointments.Any(a => a.ResourceId == resource.Id && a.StartUtc < slotEndUtc && a.EndUtc > slotStartUtc);

                    if (!blocked && slotStartUtc > DateTimeOffset.UtcNow)
                        slots.Add(new AvailableSlotDto(resource.Id, resource.Name, slotStartUtc, slotEndUtc));

                    cursorLocal += service.Duration;
                }
            }
        }

        return slots.OrderBy(s => s.StartUtc).Take(MaxSlotsReturned).ToList();
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
