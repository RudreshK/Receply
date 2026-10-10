using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Scheduling;
using Receply.Domain.Tenancy;

namespace Receply.Application.Scheduling.Queries.GetAvailableSlots;

/// <summary>
/// Finds open slots for a service on a given day. A resource linked to a real Staff account
/// (Resource.StaffId) only offers this service if that staff member has a matching StaffService
/// row - at that staff member's own session duration - and is excluded entirely for any time
/// covered by a StaffLeave on this date. A resource with no StaffId (a room/bay/equipment, or a
/// staff-type resource never linked to an actual account) keeps the old fallback: able to perform
/// any service, at the service's own default duration.
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

        // .ToUniversalTime() is required, not cosmetic: Npgsql rejects a DateTimeOffset parameter
        // whose Offset isn't exactly zero for a "timestamp with time zone" column, even though the
        // instant it represents is correct either way.
        var dayStartUtc = new DateTimeOffset(request.Date.ToDateTime(TimeOnly.MinValue), timeZone.GetUtcOffset(request.Date.ToDateTime(TimeOnly.MinValue))).ToUniversalTime();
        var dayEndUtc = dayStartUtc.AddDays(1);

        var resources = await db.Resources
            .Where(r => r.TenantId == request.TenantId && r.IsActive)
            .ToListAsync(cancellationToken);

        var resourceIds = resources.Select(r => r.Id).ToList();
        var staffIds = resources.Where(r => r.StaffId.HasValue).Select(r => r.StaffId!.Value).Distinct().ToList();

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

        var staffServicesByStaffId = await db.StaffServices
            .Where(ss => staffIds.Contains(ss.StaffId) && ss.ServiceId == service.Id)
            .ToDictionaryAsync(ss => ss.StaffId, cancellationToken);

        var leavesByStaffId = await db.StaffLeaves
            .Where(l => staffIds.Contains(l.StaffId) && l.Date == request.Date)
            .ToDictionaryAsync(l => l.StaffId, cancellationToken);

        var slots = new List<AvailableSlotDto>();

        foreach (var resource in resources)
        {
            StaffService? staffService = null;
            if (resource.StaffId.HasValue)
            {
                // Linked to a real staff account: only offer this service if that staff member is
                // actually assigned to it, and always at their own session duration.
                if (!staffServicesByStaffId.TryGetValue(resource.StaffId.Value, out staffService))
                    continue;
            }

            var duration = staffService?.SessionDuration ?? service.Duration;
            var leave = resource.StaffId.HasValue ? leavesByStaffId.GetValueOrDefault(resource.StaffId.Value) : null;

            foreach (var rule in rules.Where(r => r.ResourceId == resource.Id))
            {
                var cursorLocal = request.Date.ToDateTime(rule.StartTime);
                var ruleEndLocal = request.Date.ToDateTime(rule.EndTime);

                while (cursorLocal + duration <= ruleEndLocal)
                {
                    var slotStartUtc = new DateTimeOffset(cursorLocal, timeZone.GetUtcOffset(cursorLocal)).ToUniversalTime();
                    var slotEndUtc = slotStartUtc + duration;
                    var slotEndLocal = cursorLocal + duration;

                    var blocked = timeBlocks.Any(b => b.ResourceId == resource.Id && b.StartUtc < slotEndUtc && b.EndUtc > slotStartUtc)
                        || existingAppointments.Any(a => a.ResourceId == resource.Id && a.StartUtc < slotEndUtc && a.EndUtc > slotStartUtc)
                        || (leave?.Covers(TimeOnly.FromDateTime(cursorLocal), TimeOnly.FromDateTime(slotEndLocal)) ?? false);

                    if (!blocked && slotStartUtc > DateTimeOffset.UtcNow)
                        slots.Add(new AvailableSlotDto(resource.Id, resource.Name, slotStartUtc, slotEndUtc));

                    cursorLocal += duration;
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
