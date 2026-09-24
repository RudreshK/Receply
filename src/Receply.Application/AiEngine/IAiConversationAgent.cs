using Receply.Application.AiEngine.Contracts;

namespace Receply.Application.AiEngine;

/// <summary>
/// One turn of an LLM conversation, with tool-calling support. Implemented in Infrastructure
/// against a specific provider (Claude today). The multi-turn tool-calling loop itself lives in
/// GenerateAiReplyCommandHandler, not here - this is just "ask the model what to do next".
/// </summary>
public interface IAiConversationAgent
{
    Task<AiModelTurn> GetNextTurnAsync(AiConversationRequest request, CancellationToken cancellationToken = default);
}
