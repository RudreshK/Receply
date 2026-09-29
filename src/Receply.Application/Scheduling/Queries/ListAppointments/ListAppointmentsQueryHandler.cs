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
            select new
            {
                appointment.Id,
                ClientId = client.Id,
                client.FullName,
                client.PhoneNumber,
                ServiceName = service.Name,
                ResourceName = resource.Name,
                appointment.BranchId,
                appointment.StartUtc,
                appointment.EndUtc,
                Status = appointment.Status.ToString()
            };

        var rows = await query.ToListAsync(cancellationToken);

        return rows
            .Select(r => new AppointmentListItemDto(
                r.Id, r.ClientId, r.FullName ?? PhoneDisplayFormatter.Format(r.PhoneNumber), PhoneDisplayFormatter.Format(r.PhoneNumber),
                r.ServiceName, r.ResourceName, r.BranchId, r.StartUtc, r.EndUtc, r.Status))
            .ToList();
    }
}
