using MediatR;

namespace Receply.Application.Conversations.Commands.TranslateMessage;

public record TranslateMessageResult(string TranslatedText, string LanguageCode);

/// <summary>Translates one message into a target language, caching the result (see MessageTranslation)
/// so the same (message, language) pair is never sent to the translation provider twice.</summary>
public record TranslateMessageCommand(Guid TenantId, Guid ConversationId, Guid MessageId, string LanguageCode) : IRequest<TranslateMessageResult>;
