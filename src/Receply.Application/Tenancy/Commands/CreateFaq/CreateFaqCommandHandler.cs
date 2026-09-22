using MediatR;
using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Tenancy.Commands.CreateFaq;

public class CreateFaqCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateFaqCommand, Guid>
{
    public async Task<Guid> Handle(CreateFaqCommand request, CancellationToken cancellationToken)
    {
        var faq = Faq.Create(request.TenantId, request.Question, request.Answer);
        db.Faqs.Add(faq);
        await db.SaveChangesAsync(cancellationToken);
        return faq.Id;
    }
}
