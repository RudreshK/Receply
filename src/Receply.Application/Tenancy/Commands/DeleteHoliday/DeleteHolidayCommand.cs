using MediatR;

namespace Receply.Application.Tenancy.Commands.DeleteHoliday;

public record DeleteHolidayCommand(Guid TenantId, Guid HolidayId) : IRequest;
