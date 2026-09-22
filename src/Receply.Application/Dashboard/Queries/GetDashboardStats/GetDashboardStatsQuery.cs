using MediatR;

namespace Receply.Application.Dashboard.Queries.GetDashboardStats;

public record DashboardStatsDto(int TotalClients, int NewClientsThisMonth, int NewClientsLastMonth);

public record GetDashboardStatsQuery(Guid TenantId) : IRequest<DashboardStatsDto>;
