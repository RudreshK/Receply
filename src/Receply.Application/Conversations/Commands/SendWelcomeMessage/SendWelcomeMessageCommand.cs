using MediatR;

namespace Receply.Application.Conversations.Commands.SendWelcomeMessage;

/// <summary>Sends a guaranteed warm greeting the moment a customer starts a new conversation, ahead of the AI's own reply.</summary>
public record SendWelcomeMessageCommand(Guid TenantId, Guid ConversationId) : IRequest;
