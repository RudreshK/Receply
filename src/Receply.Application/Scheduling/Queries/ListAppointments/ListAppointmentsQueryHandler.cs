using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Scheduling.Queries.ListAppointments;

public class ListAppointmentsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListAppointmentsQuery, List<AppointmentListItemDto>>
{
    public async Task<List<AppointmentListItemDto>> Handle(ListAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var query =
            from appointment in db.Appointments
            join client in db.Clients on appointment.ClientId equals client.Id
            join service in db.Services on appointment.ServiceId equals service.Id
            join resource in db.Resources on appointment.ResourceId equals resource.Id
            where appointment.TenantId == request.TenantId
                  && appointment.StartUtc < request.To
                  && appointment.EndUtc > request.From
                  && (request.BranchId == null || appointment.BranchId == request.BranchId)
                  && (request.Status == null || appointment.Status == request.Status)
            orderby appointment.StartUtc
            select new AppointmentListItemDto(
                appointment.Id, client.FullName ?? client.PhoneNumber, service.Name, resource.Name, appointment.BranchId,
                appointment.StartUtc, appointment.EndUtc, appointment.Status.ToString());

        return await query.ToListAsync(cancellationToken);
    }
}
