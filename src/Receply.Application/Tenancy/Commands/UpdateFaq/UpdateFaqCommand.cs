using MediatR;

namespace Receply.Application.Tenancy.Commands.UpdateFaq;

public record UpdateFaqCommand(Guid TenantId, Guid FaqId, string Question, string Answer, bool IsActive) : IRequest;
