using MediatR;

namespace Receply.Application.Platform.Commands.CreateEnquiry;

public record CreateEnquiryCommand(
    string BusinessName,
    string BusinessType,
    string ContactName,
    string PhoneNumber,
    string? Email,
    string? Notes) : IRequest<Guid>;
