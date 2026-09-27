using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Receply.Application.AiEngine;
using Receply.Application.AiEngine.Contracts;

namespace Receply.Infrastructure.AiEngine;

/// <summary>IAiConversationAgent implementation against OpenAI's Chat Completions API.</summary>
public class OpenAiConversationAgent : IAiConversationAgent
{
    private readonly ChatClient _client;
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiConversationAgent> _logger;

    public OpenAiConversationAgent(IOptions<OpenAiOptions> options, ILogger<OpenAiConversationAgent> logger)
    {
        _options = options.Value;
        _logger = logger;
        _client = new ChatClient(model: _options.Model, apiKey: _options.ApiKey);
    }

    public async Task<AiModelTurn> GetNextTurnAsync(AiConversationRequest request, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage> { new SystemChatMessage(request.SystemPrompt) };
        messages.AddRange(request.Messages.SelectMany(ToChatMessages));

        var chatOptions = new ChatCompletionOptions { MaxOutputTokenCount = _options.MaxTokens };
        foreach (var tool in request.Tools)
        {
            chatOptions.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: tool.Name,
                functionDescription: tool.Description,
                functionParameters: BinaryData.FromString(tool.InputSchemaJson)));
        }

        var response = (await _client.CompleteChatAsync(messages, chatOptions, cancellationToken)).Value;

        if (response.FinishReason == ChatFinishReason.ContentFilter)
        {
            _logger.LogWarning("OpenAI refused to respond (content filter triggered).");
            return new AiModelTurn([new AiTextBlock("Sorry, I'm not able to help with that - let me connect you with a team member.")]);
        }

        var content = new List<AiContentBlock>();
        foreach (var part in response.Content)
        {
            if (part.Kind == ChatMessageContentPartKind.Text)
                content.Add(new AiTextBlock(part.Text));
        }

        foreach (var toolCall in response.ToolCalls)
            content.Add(new AiToolUseBlock(toolCall.Id, toolCall.FunctionName, toolCall.FunctionArguments.ToString()));

        return new AiModelTurn(content);
    }

    private static IEnumerable<ChatMessage> ToChatMessages(AiMessage message)
    {
        if (message.Role == AiMessageRole.User)
        {
            var toolResults = message.Content.OfType<AiToolResultBlock>().ToList();
            if (toolResults.Count > 0)
            {
                foreach (var toolResult in toolResults)
                    yield return new ToolChatMessage(toolResult.ToolUseId, toolResult.ResultJson);
                yield break;
            }

            var text = string.Concat(message.Content.OfType<AiTextBlock>().Select(t => t.Text));
            yield return new UserChatMessage(text);
            yield break;
        }

        var toolUses = message.Content.OfType<AiToolUseBlock>().ToList();
        var textBlocks = message.Content.OfType<AiTextBlock>().ToList();

        if (toolUses.Count == 0)
        {
            yield return new AssistantChatMessage(string.Concat(textBlocks.Select(t => t.Text)));
            yield break;
        }

        var toolCalls = toolUses
            .Select(tu => ChatToolCall.CreateFunctionToolCall(tu.Id, tu.Name, BinaryData.FromString(tu.ArgumentsJson)))
            .ToList();
        var assistantMessage = new AssistantChatMessage(toolCalls);
        foreach (var text in textBlocks)
            assistantMessage.Content.Add(ChatMessageContentPart.CreateTextPart(text.Text));

        yield return assistantMessage;
    }
}
