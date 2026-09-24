namespace Receply.Application.AiEngine.Contracts;

/// <summary>
/// Provider-agnostic mirror of an LLM content block (text / tool_use / tool_result). Modeled after
/// Anthropic's Messages API content-block shape so the Infrastructure-side agent implementation is
/// a near 1:1 mapping, without leaking any Anthropic-specific types into Application.
/// </summary>
public abstract record AiContentBlock;

public record AiTextBlock(string Text) : AiContentBlock;

public record AiToolUseBlock(string Id, string Name, string ArgumentsJson) : AiContentBlock;

public record AiToolResultBlock(string ToolUseId, string ResultJson, bool IsError = false) : AiContentBlock;
