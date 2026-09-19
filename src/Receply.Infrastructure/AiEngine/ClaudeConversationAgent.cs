using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Receply.Application.AiEngine;
using Receply.Application.AiEngine.Contracts;

namespace Receply.Infrastructure.AiEngine;

/// <summary>IAiConversationAgent implementation against Anthropic's Messages API (Claude).</summary>
public class ClaudeConversationAgent : IAiConversationAgent
{
    private readonly AnthropicClient _client;
    private readonly ClaudeOptions _options;
    private readonly ILogger<ClaudeConversationAgent> _logger;

    public ClaudeConversationAgent(IOptions<ClaudeOptions> options, ILogger<ClaudeConversationAgent> logger)
    {
        _options = options.Value;
        _logger = logger;
        _client = new AnthropicClient { ApiKey = _options.ApiKey };
    }

    public async Task<AiModelTurn> GetNextTurnAsync(AiConversationRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _client.Messages.Create(new MessageCreateParams
        {
            Model = _options.Model,
            MaxTokens = _options.MaxTokens,
            System = request.SystemPrompt,
            Messages = request.Messages.Select(ToMessageParam).ToList(),
            Tools = request.Tools.Select(t => (ToolUnion)new Tool
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = BuildInputSchema(t.InputSchemaJson)
            }).ToList()
        }, cancellationToken: cancellationToken);

        if (response.StopReason == "refusal")
        {
            _logger.LogWarning("Claude refused to respond (conversation may have hit a safety category).");
            return new AiModelTurn([new AiTextBlock("Sorry, I'm not able to help with that - let me connect you with a team member.")]);
        }

        var content = new List<AiContentBlock>();
        foreach (var block in response.Content)
        {
            if (block.TryPickText(out TextBlock? text))
                content.Add(new AiTextBlock(text.Text));
            else if (block.TryPickToolUse(out ToolUseBlock? toolUse))
                content.Add(new AiToolUseBlock(toolUse.ID, toolUse.Name, JsonSerializer.Serialize(toolUse.Input)));
        }

        return new AiModelTurn(content);
    }

    private static MessageParam ToMessageParam(AiMessage message) => new()
    {
        Role = message.Role == AiMessageRole.User ? Role.User : Role.Assistant,
        Content = message.Content.Select(ToContentBlockParam).ToList()
    };

    private static ContentBlockParam ToContentBlockParam(AiContentBlock block) => block switch
    {
        AiTextBlock text => new TextBlockParam { Text = text.Text },
        AiToolUseBlock toolUse => new ToolUseBlockParam
        {
            ID = toolUse.Id,
            Name = toolUse.Name,
            Input = ParseInputObject(toolUse.ArgumentsJson)
        },
        AiToolResultBlock toolResult => new ToolResultBlockParam
        {
            ToolUseID = toolResult.ToolUseId,
            Content = toolResult.ResultJson,
            IsError = toolResult.IsError
        },
        _ => throw new NotSupportedException($"Unsupported content block type '{block.GetType().Name}'.")
    };

    private static InputSchema BuildInputSchema(string schemaJson)
    {
        using var doc = JsonDocument.Parse(schemaJson);
        var root = doc.RootElement;

        var properties = new Dictionary<string, JsonElement>();
        if (root.TryGetProperty("properties", out var propsElement))
        {
            foreach (var prop in propsElement.EnumerateObject())
                properties[prop.Name] = prop.Value.Clone();
        }

        var required = root.TryGetProperty("required", out var requiredElement)
            ? requiredElement.EnumerateArray().Select(e => e.GetString()!).ToList()
            : [];

        return new InputSchema { Properties = properties, Required = required };
    }

    private static Dictionary<string, JsonElement> ParseInputObject(string json)
    {
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        return doc.RootElement.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.Clone());
    }
}
