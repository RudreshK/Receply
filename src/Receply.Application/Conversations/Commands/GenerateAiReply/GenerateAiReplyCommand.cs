using MediatR;

namespace Receply.Application.Conversations.Commands.GenerateAiReply;

/// <summary>
/// Runs after an inbound message has already been persisted to the conversation. Generates (and
/// sends) the AI's reply, or does nothing if a human has already taken over. Dispatched from the
/// background queue in Receply.Api so the WhatsApp webhook itself returns immediately.
/// </summary>
public record GenerateAiReplyCommand(Guid TenantId, Guid ConversationId) : IRequest;
