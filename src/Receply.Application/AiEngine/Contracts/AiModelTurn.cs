namespace Receply.Application.AiEngine.Contracts;

/// <summary>One model turn's raw content - some mix of text and/or tool_use blocks.</summary>
public record AiModelTurn(IReadOnlyList<AiContentBlock> Content)
{
    public string? GetText() =>
        Content.OfType<AiTextBlock>().Select(t => t.Text).FirstOrDefault();

    public IReadOnlyList<AiToolUseBlock> GetToolUses() =>
        Content.OfType<AiToolUseBlock>().ToList();
}
