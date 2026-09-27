namespace Receply.Infrastructure.AiEngine;

public class OpenAiOptions
{
    public const string SectionName = "OpenAi";

    public string ApiKey { get; set; } = default!;

    /// <summary>Pinned to Luna per explicit instruction - cheapest current OpenAI tier.</summary>
    public string Model { get; set; } = "gpt-6-luna";

    /// <summary>Short WhatsApp replies don't need much room - keep this modest to bound latency and cost.</summary>
    public int MaxTokens { get; set; } = 1024;
}
