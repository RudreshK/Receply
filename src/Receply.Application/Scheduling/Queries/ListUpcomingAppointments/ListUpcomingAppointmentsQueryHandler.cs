using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Scheduling;

namespace Receply.Application.Scheduling.Queries.ListUpcomingAppointments;

public class ListUpcomingAppointmentsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListUpcomingAppointmentsQuery, IReadOnlyList<UpcomingAppointmentDto>>
{
    public async Task<IReadOnlyList<UpcomingAppointmentDto>> Handle(ListUpcomingAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var query =
            from appointment in db.Appointments
            join service in db.Services on appointment.ServiceId equals service.Id
            where appointment.TenantId == request.TenantId
                  && appointment.ClientId == request.ClientId
                  && appointment.StartUtc >= now
                  && appointment.Status != AppointmentStatus.Cancelled
                  && appointment.Status != AppointmentStatus.Completed
            orderby appointment.StartUtc
            select new UpcomingAppointmentDto(appointment.Id, service.Name, appointment.StartUtc, appointment.EndUtc, appointment.Status.ToString());

        return await query.ToListAsync(cancellationToken);
    }
}
