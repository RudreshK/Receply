using MediatR;

namespace Receply.Application.Platform.Commands.GenerateLandingChatReply;

public record LandingChatMessageDto(string Role, string Text);

public record LandingChatReplyDto(string Reply, bool EnquirySubmitted);

public record GenerateLandingChatReplyCommand(IReadOnlyList<LandingChatMessageDto> History, string Message) : IRequest<LandingChatReplyDto>;
