using MediatR;

namespace Receply.Application.Tenancy.Queries.ListHolidays;

public record HolidayDto(Guid Id, Guid? BranchId, DateOnly Date, string Name);

public record ListHolidaysQuery(Guid TenantId, DateOnly From, DateOnly To) : IRequest<List<HolidayDto>>;
