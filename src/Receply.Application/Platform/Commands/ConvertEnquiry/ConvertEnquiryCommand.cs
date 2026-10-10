using MediatR;

namespace Receply.Application.Platform.Commands.ConvertEnquiry;

public record ConvertEnquiryCommand(Guid EnquiryId, string TimeZoneId) : IRequest<Guid>;
