using Receply.Domain.Common;

namespace Receply.Domain.Conversations;

/// <summary>
/// A cached translation of one Message into one language, requested on demand from the dashboard
/// (a staff member's "Translate" click). Keyed by (MessageId, LanguageCode) so the same translation
/// is never paid for twice, whether the same staff member re-opens the chat or a different staff
/// member with the same preferred language views it.
/// </summary>
public class MessageTranslation : TenantOwnedEntity
{
    public Guid MessageId { get; private set; }
    public string LanguageCode { get; private set; } = default!;
    public string TranslatedText { get; private set; } = default!;

    private MessageTranslation() { }

    public static MessageTranslation Create(Guid tenantId, Guid messageId, string languageCode, string translatedText)
    {
        var translation = new MessageTranslation
        {
            MessageId = messageId,
            LanguageCode = languageCode,
            TranslatedText = translatedText
        };
        translation.TenantId = tenantId;
        return translation;
    }
}
