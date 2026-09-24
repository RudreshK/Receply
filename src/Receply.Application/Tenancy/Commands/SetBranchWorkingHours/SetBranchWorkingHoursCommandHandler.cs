using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Tenancy.Commands.SetBranchWorkingHours;

public class SetBranchWorkingHoursCommandHandler(IApplicationDbContext db) : IRequestHandler<SetBranchWorkingHoursCommand>
{
    public async Task Handle(SetBranchWorkingHoursCommand request, CancellationToken cancellationToken)
    {
        var branchExists = await db.Branches.AnyAsync(b => b.Id == request.BranchId && b.TenantId == request.TenantId, cancellationToken);
        if (!branchExists)
            throw new KeyNotFoundException($"Branch '{request.BranchId}' not found.");

        var existing = await db.BranchWorkingHours
            .Where(h => h.TenantId == request.TenantId && h.BranchId == request.BranchId)
            .ToListAsync(cancellationToken);
        db.BranchWorkingHours.RemoveRange(existing);

        foreach (var entry in request.Entries)
        {
            db.BranchWorkingHours.Add(BranchWorkingHours.Create(
                request.TenantId, request.BranchId, entry.DayOfWeek, entry.OpenTime, entry.CloseTime, entry.IsClosed));
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
