using MediatR;
using Receply.Application.Staffing.Commands.AddStaff;

namespace Receply.Application.Staffing.Commands.UpdateStaff;

public record UpdateStaffCommand(
    Guid TenantId,
    Guid? ActingStaffId,
    Guid StaffId,
    string FullName,
    string Email,
    string? Designation,
    int ExperienceYears,
    IReadOnlyList<StaffServiceInput> Services) : IRequest;
