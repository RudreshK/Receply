using MediatR;

namespace Receply.Application.Conversations.Commands.RequestHandoff;

public record RequestHandoffCommand(Guid ConversationId, string Reason) : IRequest;
