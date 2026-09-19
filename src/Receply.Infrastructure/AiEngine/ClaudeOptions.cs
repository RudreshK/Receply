namespace Receply.Infrastructure.AiEngine;

public class ClaudeOptions
{
    public const string SectionName = "Anthropic";

    public string ApiKey { get; set; } = default!;

    /// <summary>Defaults to Anthropic's most capable model; override here (e.g. to a cheaper model) once volume/cost tuning matters.</summary>
    public string Model { get; set; } = "claude-opus-5";

    /// <summary>Short WhatsApp replies don't need much room - keep this modest to bound latency and cost.</summary>
    public int MaxTokens { get; set; } = 1024;
}
