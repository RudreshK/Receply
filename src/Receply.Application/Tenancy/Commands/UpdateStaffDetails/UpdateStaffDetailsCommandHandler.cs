using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Tenancy.Commands.UpdateStaffDetails;

public class UpdateStaffDetailsCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateStaffDetailsCommand>
{
    public async Task Handle(UpdateStaffDetailsCommand request, CancellationToken cancellationToken)
    {
        var staff = await db.Staff.FirstOrDefaultAsync(s => s.Id == request.StaffId && s.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Staff '{request.StaffId}' not found.");

        var phoneNumber = request.PhoneNumber ?? staff.PhoneNumber;
        if (phoneNumber != staff.PhoneNumber && await db.Staff.AnyAsync(s => s.PhoneNumber == phoneNumber, cancellationToken))
            throw new InvalidOperationException($"Phone number '{phoneNumber}' is already in use by another staff member.");

        staff.UpdateOwnDetails(request.FullName ?? staff.FullName, request.Email ?? staff.Email, phoneNumber);
        await db.SaveChangesAsync(cancellationToken);
    }
}
