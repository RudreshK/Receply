using MediatR;

namespace Receply.Application.Tenancy.Commands.DeleteFaq;

public record DeleteFaqCommand(Guid TenantId, Guid FaqId) : IRequest;
