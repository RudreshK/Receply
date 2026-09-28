using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Application.Translation;
using Receply.Domain.Conversations;

namespace Receply.Application.Conversations.Commands.TranslateMessage;

public class TranslateMessageCommandHandler(IApplicationDbContext db, ITranslationService translationService)
    : IRequestHandler<TranslateMessageCommand, TranslateMessageResult>
{
    public async Task<TranslateMessageResult> Handle(TranslateMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await db.Messages.FirstOrDefaultAsync(
            m => m.Id == request.MessageId && m.ConversationId == request.ConversationId && m.TenantId == request.TenantId,
            cancellationToken)
            ?? throw new KeyNotFoundException($"Message '{request.MessageId}' not found.");

        var cached = await db.MessageTranslations.FirstOrDefaultAsync(
            t => t.MessageId == message.Id && t.LanguageCode == request.LanguageCode, cancellationToken);

        if (cached is not null)
            return new TranslateMessageResult(cached.TranslatedText, cached.LanguageCode);

        var translatedText = await translationService.TranslateAsync(message.Body, request.LanguageCode, cancellationToken);

        db.MessageTranslations.Add(MessageTranslation.Create(request.TenantId, message.Id, request.LanguageCode, translatedText));
        await db.SaveChangesAsync(cancellationToken);

        return new TranslateMessageResult(translatedText, request.LanguageCode);
    }
}
