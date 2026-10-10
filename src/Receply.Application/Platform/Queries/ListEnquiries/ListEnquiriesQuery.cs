using MediatR;

namespace Receply.Application.Platform.Queries.ListEnquiries;

public record EnquiryListItemDto(
    Guid Id,
    string BusinessName,
    string BusinessType,
    string ContactName,
    string PhoneNumber,
    string? Email,
    string? Notes,
    string Status,
    Guid? ConvertedTenantId,
    DateTimeOffset CreatedOn);

public record ListEnquiriesQuery : IRequest<List<EnquiryListItemDto>>;
