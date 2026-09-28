namespace Receply.Application.Translation;

/// <summary>Translates a single piece of chat text into a target language. Implemented against
/// whichever provider is configured (OpenAI today) - Application code only depends on this seam.</summary>
public interface ITranslationService
{
    Task<string> TranslateAsync(string text, string targetLanguageCode, CancellationToken cancellationToken = default);
}
