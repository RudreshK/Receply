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

        var endUtc = request.StartUtc + service.Duration;

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
            request.StartUtc, service.Duration);

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }
}
