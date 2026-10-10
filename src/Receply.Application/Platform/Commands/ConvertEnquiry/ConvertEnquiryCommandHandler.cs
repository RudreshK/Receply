using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Platform;
using Receply.Domain.Tenancy;

namespace Receply.Application.Platform.Commands.ConvertEnquiry;

/// <summary>Turns an enquiry logged by hand into a real tenant + Owner, the same way self-serve
/// signup does (Tenant.Create + a Main Branch + an Owner Staff row) - just triggered by a trusted
/// platform admin instead of an OTP the business itself verified.</summary>
public class ConvertEnquiryCommandHandler(IApplicationDbContext db) : IRequestHandler<ConvertEnquiryCommand, Guid>
{
    public async Task<Guid> Handle(ConvertEnquiryCommand request, CancellationToken cancellationToken)
    {
        var enquiry = await db.BusinessEnquiries.FirstOrDefaultAsync(e => e.Id == request.EnquiryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Enquiry '{request.EnquiryId}' not found.");

        if (enquiry.Status != EnquiryStatus.New)
            throw new InvalidOperationException("This enquiry has already been resolved.");

        if (await db.Staff.AnyAsync(s => s.PhoneNumber == enquiry.PhoneNumber, cancellationToken))
            throw new InvalidOperationException($"Phone number '{enquiry.PhoneNumber}' is already in use by another staff member.");

        var tenant = Tenant.Create(enquiry.BusinessName, enquiry.BusinessType, request.TimeZoneId);
        tenant.AddBranch("Main Branch", "");
        var owner = Staff.Create(tenant.Id, enquiry.ContactName, enquiry.Email ?? "", enquiry.PhoneNumber, StaffRole.Owner);

        db.Tenants.Add(tenant);
        db.Staff.Add(owner);
        enquiry.MarkConverted(tenant.Id);

        await db.SaveChangesAsync(cancellationToken);

        return tenant.Id;
    }
}
