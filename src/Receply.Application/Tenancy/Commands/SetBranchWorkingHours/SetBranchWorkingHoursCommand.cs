using MediatR;

namespace Receply.Application.Tenancy.Commands.SetBranchWorkingHours;

public record BranchWorkingHoursEntry(DayOfWeek DayOfWeek, TimeOnly? OpenTime, TimeOnly? CloseTime, bool IsClosed);

public record SetBranchWorkingHoursCommand(Guid TenantId, Guid BranchId, List<BranchWorkingHoursEntry> Entries) : IRequest;
