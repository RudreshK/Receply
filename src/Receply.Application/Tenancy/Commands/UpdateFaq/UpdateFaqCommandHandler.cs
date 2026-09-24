using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Tenancy.Commands.UpdateFaq;

public class UpdateFaqCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateFaqCommand>
{
    public async Task Handle(UpdateFaqCommand request, CancellationToken cancellationToken)
    {
        var faq = await db.Faqs.FirstOrDefaultAsync(f => f.Id == request.FaqId && f.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"FAQ '{request.FaqId}' not found.");

        faq.Update(request.Question, request.Answer);
        if (request.IsActive)
            faq.Activate();
        else
            faq.Deactivate();

        await db.SaveChangesAsync(cancellationToken);
    }
}
