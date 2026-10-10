using MediatR;

namespace Receply.Application.Staffing.Queries.ListStaffLeaves;

public record StaffLeaveDto(Guid Id, Guid StaffId, DateOnly Date, TimeOnly? StartTime, TimeOnly? EndTime, string? Reason);

public record ListStaffLeavesQuery(Guid TenantId, Guid StaffId, DateOnly From, DateOnly To) : IRequest<IReadOnlyList<StaffLeaveDto>>;
