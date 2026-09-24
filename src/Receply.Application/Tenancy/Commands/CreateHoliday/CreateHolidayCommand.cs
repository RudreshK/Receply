using MediatR;

namespace Receply.Application.Tenancy.Commands.CreateHoliday;

public record CreateHolidayCommand(Guid TenantId, Guid? BranchId, DateOnly Date, string Name) : IRequest<Guid>;
