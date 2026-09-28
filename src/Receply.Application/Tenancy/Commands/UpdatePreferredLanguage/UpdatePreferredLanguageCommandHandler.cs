using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Tenancy.Commands.UpdatePreferredLanguage;

public class UpdatePreferredLanguageCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdatePreferredLanguageCommand>
{
    public async Task Handle(UpdatePreferredLanguageCommand request, CancellationToken cancellationToken)
    {
        var staff = await db.Staff.FirstOrDefaultAsync(s => s.Id == request.StaffId && s.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Staff '{request.StaffId}' not found.");

        staff.SetPreferredLanguage(request.LanguageCode);
        await db.SaveChangesAsync(cancellationToken);
    }
}
