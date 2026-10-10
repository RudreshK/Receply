using MediatR;

namespace Receply.Application.Platform.Queries.GetPlatformStats;

public record PlatformStatsDto(int TotalBusinesses, int NewThisWeek, int NewThisMonth, int PendingEnquiries);

public record GetPlatformStatsQuery : IRequest<PlatformStatsDto>;
