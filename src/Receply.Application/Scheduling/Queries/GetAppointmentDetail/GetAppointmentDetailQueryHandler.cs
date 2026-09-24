using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Scheduling.Queries.GetAppointmentDetail;

public class GetAppointmentDetailQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetAppointmentDetailQuery, AppointmentDetailDto>
{
    public async Task<AppointmentDetailDto> Handle(GetAppointmentDetailQuery request, CancellationToken cancellationToken)
    {
        var query =
            from appointment in db.Appointments
            join client in db.Clients on appointment.ClientId equals client.Id
            join service in db.Services on appointment.ServiceId equals service.Id
            join resource in db.Resources on appointment.ResourceId equals resource.Id
            join branch in db.Branches on appointment.BranchId equals branch.Id
            where appointment.TenantId == request.TenantId && appointment.Id == request.AppointmentId
            select new AppointmentDetailDto(
                appointment.Id, client.Id, client.FullName ?? client.PhoneNumber, client.PhoneNumber,
                service.Id, service.Name, resource.Id, resource.Name, branch.Id, branch.Name,
                appointment.StartUtc, appointment.EndUtc, appointment.Status.ToString(), appointment.CancellationReason);

        return await query.FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{request.AppointmentId}' not found.");
    }
}
