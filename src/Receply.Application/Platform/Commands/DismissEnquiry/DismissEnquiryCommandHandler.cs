using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Platform.Commands.DismissEnquiry;

public class DismissEnquiryCommandHandler(IApplicationDbContext db) : IRequestHandler<DismissEnquiryCommand>
{
    public async Task Handle(DismissEnquiryCommand request, CancellationToken cancellationToken)
    {
        var enquiry = await db.BusinessEnquiries.FirstOrDefaultAsync(e => e.Id == request.EnquiryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Enquiry '{request.EnquiryId}' not found.");

        enquiry.Dismiss();
        await db.SaveChangesAsync(cancellationToken);
    }
}
