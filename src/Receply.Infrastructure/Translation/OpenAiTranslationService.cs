using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Receply.Application.Translation;
using Receply.Infrastructure.AiEngine;

namespace Receply.Infrastructure.Translation;

/// <summary>ITranslationService implementation against OpenAI's Chat Completions API - reuses the
/// same OpenAiOptions (model, key) as the conversation agent, since it's the same account/billing,
/// just a different, much simpler, one-shot prompt with no tools or conversation history.</summary>
public class OpenAiTranslationService : ITranslationService
{
    private readonly ChatClient _client;

    public OpenAiTranslationService(IOptions<OpenAiOptions> options)
    {
        _client = new ChatClient(model: options.Value.Model, apiKey: options.Value.ApiKey);
    }

    public async Task<string> TranslateAsync(string text, string targetLanguageCode, CancellationToken cancellationToken = default)
    {
        var languageName = LanguageNames.GetValueOrDefault(targetLanguageCode, targetLanguageCode);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(
                $"You translate WhatsApp business chat messages into {languageName}. " +
                "Reply with ONLY the translated text - no quotes, no explanation, no original text alongside it."),
            new UserChatMessage(text)
        };

        var response = (await _client.CompleteChatAsync(messages, cancellationToken: cancellationToken)).Value;

        return string.Concat(response.Content
            .Where(part => part.Kind == ChatMessageContentPartKind.Text)
            .Select(part => part.Text)).Trim();
    }

    private static readonly Dictionary<string, string> LanguageNames = new()
    {
        ["en"] = "English",
        ["hi"] = "Hindi",
        ["te"] = "Telugu",
        ["ta"] = "Tamil",
        ["kn"] = "Kannada",
        ["ml"] = "Malayalam",
        ["bn"] = "Bengali",
        ["mr"] = "Marathi",
        ["gu"] = "Gujarati",
        ["pa"] = "Punjabi",
        ["ur"] = "Urdu",
    };
}
