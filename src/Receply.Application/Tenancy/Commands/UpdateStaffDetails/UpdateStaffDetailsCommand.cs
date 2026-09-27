using MediatR;

namespace Receply.Application.Tenancy.Commands.UpdateStaffDetails;

/// <summary>Null fields are left unchanged - only the fields the caller actually wants to change need to be sent.</summary>
public record UpdateStaffDetailsCommand(Guid TenantId, Guid StaffId, string? FullName, string? Email, string? PhoneNumber) : IRequest;
