using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Tenancy.Commands.DeleteHoliday;

public class DeleteHolidayCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteHolidayCommand>
{
    public async Task Handle(DeleteHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = await db.Holidays.FirstOrDefaultAsync(h => h.Id == request.HolidayId && h.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Holiday '{request.HolidayId}' not found.");

        holiday.Delete();
        await db.SaveChangesAsync(cancellationToken);
    }
}
