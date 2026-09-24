namespace Receply.Application.AiEngine.Contracts;

public record AiConversationRequest(
    string SystemPrompt,
    IReadOnlyList<AiMessage> Messages,
    IReadOnlyList<AiTool> Tools);
