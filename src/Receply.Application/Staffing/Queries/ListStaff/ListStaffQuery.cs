using MediatR;

namespace Receply.Application.Staffing.Queries.ListStaff;

public record StaffServiceDto(Guid ServiceId, string ServiceName, int SessionDurationMinutes);

public record StaffDto(
    Guid Id,
    string FullName,
    string Email,
    string PhoneNumber,
    string Role,
    Guid? BranchId,
    string? Designation,
    int ExperienceYears,
    bool IsActive,
    IReadOnlyList<StaffServiceDto> Services);

public record ListStaffQuery(Guid TenantId, Guid? BranchId) : IRequest<IReadOnlyList<StaffDto>>;
