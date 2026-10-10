using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Scheduling;

namespace Receply.Application.Dashboard.Queries.GetDashboardStats;

/// <summary>
/// "Pending" = booked or rescheduled (not yet cancelled/completed/no-show) - the work still sitting
/// on someone's calendar. BranchId, when given, scopes bookings and the per-staff breakdown to that
/// one branch; client counts stay tenant-wide since a Client isn't tied to a branch.
/// </summary>
public class GetDashboardStatsQueryHandler(IApplicationDbContext db) : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var startOfThisMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var startOfLastMonth = startOfThisMonth.AddMonths(-1);

        var clients = db.Clients.Where(c => c.TenantId == request.TenantId);
        var totalClients = await clients.CountAsync(cancellationToken);
        var newThisMonth = await clients.CountAsync(c => c.CreatedOn >= startOfThisMonth, cancellationToken);
        var newLastMonth = await clients.CountAsync(c => c.CreatedOn >= startOfLastMonth && c.CreatedOn < startOfThisMonth, cancellationToken);

        var appointments = db.Appointments.Where(a =>
            a.TenantId == request.TenantId && (request.BranchId == null || a.BranchId == request.BranchId));

        var totalBookings = await appointments.CountAsync(cancellationToken);
        var upcomingBookings = await appointments.CountAsync(
            a => a.StartUtc > now && (a.Status == AppointmentStatus.Booked || a.Status == AppointmentStatus.Rescheduled),
            cancellationToken);

        // GroupBy + join + a custom record constructor in one query doesn't translate to SQL in
        // this EF Core version - materialize the (staff, name) pairs and group them client-side
        // instead. Dataset is one tenant's (optionally one branch's) pending appointments, small
        // enough that this is simpler than fighting the translator.
        var pendingRows = await (
            from a in appointments
            where a.Status == AppointmentStatus.Booked || a.Status == AppointmentStatus.Rescheduled
            join r in db.Resources on a.ResourceId equals r.Id
            where r.StaffId != null
            join s in db.Staff on r.StaffId equals s.Id
            select new { s.Id, s.FullName }
        ).ToListAsync(cancellationToken);

        var pendingByStaff = pendingRows
            .GroupBy(x => new { x.Id, x.FullName })
            .Select(g => new StaffPendingCountDto(g.Key.Id, g.Key.FullName, g.Count()))
            .OrderByDescending(x => x.PendingCount)
            .ToList();

        return new DashboardStatsDto(totalClients, newThisMonth, newLastMonth, totalBookings, upcomingBookings, pendingByStaff);
    }
}
