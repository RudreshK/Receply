using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Scheduling.Commands.CancelAppointment;

public class CancelAppointmentCommandHandler(IApplicationDbContext db) : IRequestHandler<CancelAppointmentCommand>
{
    public async Task Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await db.Appointments.FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{request.AppointmentId}' not found.");

        appointment.Cancel(request.Reason);

        await db.SaveChangesAsync(cancellationToken);
    }
}
