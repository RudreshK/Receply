using MediatR;
using Receply.Domain.Tenancy;

namespace Receply.Application.Staffing.Commands.AddStaff;

public record StaffServiceInput(Guid ServiceId, int SessionDurationMinutes);

public record AddStaffCommand(
    Guid TenantId,
    Guid? ActingStaffId,
    string FullName,
    string Email,
    string PhoneNumber,
    StaffRole Role,
    Guid? BranchId,
    string? Designation,
    int ExperienceYears,
    IReadOnlyList<StaffServiceInput> Services) : IRequest<Guid>;
