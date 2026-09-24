using MediatR;

namespace Receply.Application.Tenancy.Queries.GetBranchWorkingHours;

public record BranchWorkingHoursDto(DayOfWeek DayOfWeek, TimeOnly? OpenTime, TimeOnly? CloseTime, bool IsClosed);

public record GetBranchWorkingHoursQuery(Guid TenantId, Guid BranchId) : IRequest<List<BranchWorkingHoursDto>>;
