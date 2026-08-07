using Microsoft.Extensions.AI;

namespace AIBuilder;

/// <summary>
/// Dependencies shared by all requests created from a single <see cref="AIClient"/>.
/// </summary>
internal sealed class AIExecutionContext
{
    public required IChatClient ChatClient { get; init; }

    public IPromptTemplateStore? TemplateStore { get; init; }

    public IConversationStore? ConversationStore { get; init; }

    public IReadOnlyList<AITool> RegisteredTools { get; init; } = [];
}

/// <summary>
/// Mutable per-request state shared between the untyped and typed request builders.
/// </summary>
internal sealed class AIRequestState
{
    public required AIExecutionContext Context { get; init; }

    public string? Prompt { get; set; }

    public string? TemplateName { get; set; }

    public Dictionary<string, object?> Variables { get; } = [];

    public string? SystemPrompt { get; set; }

    public string? ContextText { get; set; }

    public string? ConversationId { get; set; }

    public List<AITool> Tools { get; } = [];

    public bool UseRegisteredTools { get; set; }

    public ChatOptions Options { get; } = new();
}

/// <summary>
/// A request that has been resolved into concrete messages and options.
/// </summary>
internal sealed class AIPreparedRequest
{
    public required List<ChatMessage> Messages { get; init; }

    public required ChatOptions Options { get; init; }

    public required string UserText { get; init; }
}
