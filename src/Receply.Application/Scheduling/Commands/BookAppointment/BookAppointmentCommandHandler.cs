using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Scheduling;

namespace Receply.Application.Scheduling.Commands.BookAppointment;

public class BookAppointmentCommandHandler(IApplicationDbContext db) : IRequestHandler<BookAppointmentCommand, Guid>
{
    public async Task<Guid> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
    {
        var service = await db.Services.FirstOrDefaultAsync(s => s.Id == request.ServiceId && s.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Service '{request.ServiceId}' not found.");

        var resource = await db.Resources.FirstOrDefaultAsync(r => r.Id == request.ResourceId && r.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Resource '{request.ResourceId}' not found.");

        // Mirrors GetAvailableSlotsQueryHandler: a staff-linked resource books at that staff
        // member's own session duration, and is off-limits for any time covered by their leave -
        // re-checked here (not just trusted from the earlier check_availability read) so a stale
        // read or an explicit resource_id can't slip a booking past a leave day.
        var staffService = resource.StaffId.HasValue
            ? await db.StaffServices.FirstOrDefaultAsync(ss => ss.StaffId == resource.StaffId && ss.ServiceId == request.ServiceId, cancellationToken)
            : null;
        var duration = staffService?.SessionDuration ?? service.Duration;
        var endUtc = request.StartUtc + duration;

        if (resource.StaffId.HasValue)
        {
            var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken)
                ?? throw new KeyNotFoundException($"Tenant '{request.TenantId}' not found.");
            var timeZone = ResolveTimeZone(tenant.TimeZoneId);
            var startLocal = TimeZoneInfo.ConvertTime(request.StartUtc, timeZone);
            var endLocal = TimeZoneInfo.ConvertTime(endUtc, timeZone);

            var leave = await db.StaffLeaves.FirstOrDefaultAsync(
                l => l.StaffId == resource.StaffId && l.Date == DateOnly.FromDateTime(startLocal.Date), cancellationToken);

            if (leave is not null && leave.Covers(TimeOnly.FromDateTime(startLocal.DateTime), TimeOnly.FromDateTime(endLocal.DateTime)))
                throw new InvalidOperationException("That staff member is on leave at the requested time.");
        }

        // NOTE: this only checks for a literal overlap against existing bookings. It does not yet
        // validate against the resource's AvailabilityRule/TimeBlock calendar - that's the next
        // piece of real scheduling logic to add before this goes live.
        var hasConflict = await db.Appointments.AnyAsync(a =>
            a.ResourceId == request.ResourceId &&
            a.Status != AppointmentStatus.Cancelled &&
            a.StartUtc < endUtc &&
            a.EndUtc > request.StartUtc,
            cancellationToken);

        if (hasConflict)
            throw new InvalidOperationException("The requested time slot is no longer available.");

        var appointment = Appointment.Book(
            request.TenantId, request.ClientId, request.ServiceId, request.ResourceId, request.BranchId,
            request.StartUtc, duration);

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync(cancellationToken);

        return appointment.Id;
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
