using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Platform.Queries.ListEnquiries;

public class ListEnquiriesQueryHandler(IApplicationDbContext db) : IRequestHandler<ListEnquiriesQuery, List<EnquiryListItemDto>>
{
    public async Task<List<EnquiryListItemDto>> Handle(ListEnquiriesQuery request, CancellationToken cancellationToken)
    {
        return await db.BusinessEnquiries
            .OrderByDescending(e => e.CreatedOn)
            .Select(e => new EnquiryListItemDto(
                e.Id,
                e.BusinessName,
                e.BusinessType.ToString(),
                e.ContactName,
                e.PhoneNumber,
                e.Email,
                e.Notes,
                e.Status.ToString(),
                e.ConvertedTenantId,
                e.CreatedOn))
            .ToListAsync(cancellationToken);
    }
}
