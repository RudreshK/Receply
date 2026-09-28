using MediatR;

namespace Receply.Application.Conversations.Commands.ResumeAi;

/// <summary>The dashboard's "back to AI" toggle - hands a conversation back from a human to the AI.</summary>
public record ResumeAiCommand(Guid TenantId, Guid ConversationId) : IRequest;
