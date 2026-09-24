namespace Receply.Application.AiEngine.Contracts;

/// <summary>
/// A tool the AI agent may call, described as a JSON Schema string so it's portable across LLM
/// providers - Infrastructure translates it into the specific provider SDK's tool format.
/// </summary>
public record AiTool(string Name, string Description, string InputSchemaJson);
