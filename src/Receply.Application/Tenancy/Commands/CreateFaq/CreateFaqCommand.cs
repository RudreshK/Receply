using MediatR;

namespace Receply.Application.Tenancy.Commands.CreateFaq;

public record CreateFaqCommand(Guid TenantId, string Question, string Answer) : IRequest<Guid>;
