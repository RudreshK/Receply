using MediatR;

namespace Receply.Application.Dashboard.Queries.GetDashboardStats;

public record StaffPendingCountDto(Guid StaffId, string StaffName, int PendingCount);

public record DashboardStatsDto(
    int TotalClients,
    int NewClientsThisMonth,
    int NewClientsLastMonth,
    int TotalBookings,
    int UpcomingBookings,
    IReadOnlyList<StaffPendingCountDto> PendingByStaff);

public record GetDashboardStatsQuery(Guid TenantId, Guid? BranchId) : IRequest<DashboardStatsDto>;
