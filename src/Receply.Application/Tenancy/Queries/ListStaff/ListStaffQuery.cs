using MediatR;

namespace Receply.Application.Tenancy.Queries.ListStaff;

public record StaffListItemDto(Guid Id, string FullName, string Email, string PhoneNumber, string Role, bool IsActive);

public record ListStaffQuery(Guid TenantId) : IRequest<List<StaffListItemDto>>;
