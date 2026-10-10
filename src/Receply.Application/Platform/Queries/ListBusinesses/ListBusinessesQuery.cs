using MediatR;

namespace Receply.Application.Platform.Queries.ListBusinesses;

public record BusinessListItemDto(
    Guid Id,
    string Name,
    string BusinessType,
    string Status,
    string Plan,
    bool HasActiveAccess,
    int? DaysLeft,
    DateTimeOffset CreatedOn);

public record ListBusinessesQuery : IRequest<List<BusinessListItemDto>>;
