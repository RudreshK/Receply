using MediatR;

namespace Receply.Application.Platform.Commands.DismissEnquiry;

public record DismissEnquiryCommand(Guid EnquiryId) : IRequest;
