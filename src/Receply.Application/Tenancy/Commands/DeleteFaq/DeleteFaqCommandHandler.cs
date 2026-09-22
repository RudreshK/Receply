using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Tenancy.Commands.DeleteFaq;

public class DeleteFaqCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteFaqCommand>
{
    public async Task Handle(DeleteFaqCommand request, CancellationToken cancellationToken)
    {
        var faq = await db.Faqs.FirstOrDefaultAsync(f => f.Id == request.FaqId && f.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"FAQ '{request.FaqId}' not found.");

        faq.Delete();
        await db.SaveChangesAsync(cancellationToken);
    }
}
