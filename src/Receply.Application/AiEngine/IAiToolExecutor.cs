namespace Receply.Application.AiEngine;

public record AiToolContext(Guid TenantId, Guid ConversationId, Guid CustomerId);

public interface IAiToolExecutor
{
    /// <summary>
    /// Executes one tool call and returns a JSON string result to feed back to the model as a
    /// tool_result. Never throws for expected failures (not found, validation, conflicts) - those
    /// are caught and returned as a JSON error object so the model can react to them conversationally.
    /// </summary>
    Task<(string ResultJson, bool IsError)> ExecuteAsync(string toolName, string argumentsJson, AiToolContext context, CancellationToken cancellationToken = default);
}
