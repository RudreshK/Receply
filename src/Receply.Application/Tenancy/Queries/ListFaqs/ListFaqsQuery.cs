using MediatR;

namespace Receply.Application.Tenancy.Queries.ListFaqs;

public record FaqDto(Guid Id, string Question, string Answer, bool IsActive);

public record ListFaqsQuery(Guid TenantId) : IRequest<List<FaqDto>>;
