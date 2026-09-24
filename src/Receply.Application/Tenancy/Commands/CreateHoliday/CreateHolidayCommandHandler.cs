using MediatR;
using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Tenancy.Commands.CreateHoliday;

public class CreateHolidayCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateHolidayCommand, Guid>
{
    public async Task<Guid> Handle(CreateHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = Holiday.Create(request.TenantId, request.BranchId, request.Date, request.Name);
        db.Holidays.Add(holiday);
        await db.SaveChangesAsync(cancellationToken);
        return holiday.Id;
    }
}
