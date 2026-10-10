using MediatR;
using Receply.Application.Common;
using Receply.Domain.Platform;
using Receply.Domain.Tenancy;

namespace Receply.Application.Platform.Commands.CreateEnquiry;

public class CreateEnquiryCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateEnquiryCommand, Guid>
{
    public async Task<Guid> Handle(CreateEnquiryCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<BusinessType>(request.BusinessType, ignoreCase: true, out var businessType))
            throw new ArgumentException($"Unknown business type '{request.BusinessType}'.", nameof(request.BusinessType));

        var enquiry = BusinessEnquiry.Create(
            request.BusinessName, businessType, request.ContactName, request.PhoneNumber, request.Email, request.Notes);

        db.BusinessEnquiries.Add(enquiry);
        await db.SaveChangesAsync(cancellationToken);

        return enquiry.Id;
    }
}
