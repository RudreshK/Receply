using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Scheduling;

namespace Receply.Application.Scheduling.Commands.AddAppointmentFollowUp;

public class AddAppointmentFollowUpCommandHandler(IApplicationDbContext db) : IRequestHandler<AddAppointmentFollowUpCommand>
{
    private static readonly TimeOnly SendTimeLocal = new(10, 0);

    public async Task Handle(AddAppointmentFollowUpCommand request, CancellationToken cancellationToken)
    {
        var appointment = await db.Appointments.FirstOrDefaultAsync(
            a => a.Id == request.AppointmentId && a.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{request.AppointmentId}' not found.");

        if (appointment.Status != AppointmentStatus.Completed)
            throw new InvalidOperationException("Follow-up instructions can only be added to a completed appointment.");

        var resource = await db.Resources.FirstOrDefaultAsync(r => r.Id == appointment.ResourceId, cancellationToken);
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken);

        var sentByName = resource?.Name ?? tenant?.Name ?? "our team";
        var timeZone = TryGetTimeZone(tenant?.TimeZoneId);
        var appointmentDateLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(appointment.StartUtc, timeZone).Date);

        foreach (var offset in request.DayOffsets)
        {
            var sendLocalWallClock = appointmentDateLocal.AddDays(offset).ToDateTime(SendTimeLocal);
            var sendUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(sendLocalWallClock, timeZone));

            db.AppointmentFollowUps.Add(
                AppointmentFollowUp.Schedule(request.TenantId, appointment.Id, request.Instructions, sentByName, sendUtc));
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static TimeZoneInfo TryGetTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            return TimeZoneInfo.Utc;

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
