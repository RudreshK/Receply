using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Scheduling.Commands.RescheduleAppointment;

public class RescheduleAppointmentCommandHandler(IApplicationDbContext db) : IRequestHandler<RescheduleAppointmentCommand>
{
    public async Task Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await db.Appointments.FirstOrDefaultAsync(
            a => a.Id == request.AppointmentId && a.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{request.AppointmentId}' not found.");

        var duration = appointment.EndUtc - appointment.StartUtc;
        appointment.Reschedule(request.NewStartUtc, duration);

        await db.SaveChangesAsync(cancellationToken);
    }
}
