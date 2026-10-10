using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Tenancy.Commands.CreateStaff;

public class CreateStaffCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateStaffCommand, Guid>
{
    public async Task<Guid> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<StaffRole>(request.Role, ignoreCase: true, out var role) || role == StaffRole.Owner)
            throw new ArgumentException($"Unknown or unassignable role '{request.Role}'.", nameof(request.Role));

        // Login is resolved by phone number alone (no tenant context yet at that point), so it has to
        // be unique system-wide, not just within this tenant.
        if (await db.Staff.AnyAsync(s => s.PhoneNumber == request.PhoneNumber, cancellationToken))
            throw new InvalidOperationException($"Phone number '{request.PhoneNumber}' is already in use by another staff member.");

        var email = request.Email ?? "";
        if (!string.IsNullOrWhiteSpace(email) &&
            await db.Staff.AnyAsync(s => s.TenantId == request.TenantId && s.Email == email, cancellationToken))
            throw new InvalidOperationException($"Email '{email}' is already in use by another staff member.");

        var staff = Staff.Create(request.TenantId, request.FullName, email, request.PhoneNumber, role);
        db.Staff.Add(staff);
        await db.SaveChangesAsync(cancellationToken);

        return staff.Id;
    }
}
