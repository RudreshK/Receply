using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Tenancy.Queries.ListFaqs;

public class ListFaqsQueryHandler(IApplicationDbContext db) : IRequestHandler<ListFaqsQuery, List<FaqDto>>
{
    public async Task<List<FaqDto>> Handle(ListFaqsQuery request, CancellationToken cancellationToken)
    {
        return await db.Faqs
            .Where(f => f.TenantId == request.TenantId)
            .OrderBy(f => f.Question)
            .Select(f => new FaqDto(f.Id, f.Question, f.Answer, f.IsActive))
            .ToListAsync(cancellationToken);
    }
}
