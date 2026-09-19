namespace Receply.Application.AiEngine.Contracts;

public enum AiMessageRole
{
    User,
    Assistant
}

public record AiMessage(AiMessageRole Role, IReadOnlyList<AiContentBlock> Content)
{
    public static AiMessage FromText(AiMessageRole role, string text) => new(role, [new AiTextBlock(text)]);
}
