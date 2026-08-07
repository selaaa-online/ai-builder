using Microsoft.Extensions.AI;

namespace AIBuilder;

/// <summary>
/// Default <see cref="IAIClient"/> implementation backed by an <see cref="IChatClient"/> pipeline.
/// </summary>
public sealed class AIClient : IAIClient
{
    private readonly AIExecutionContext _context;

    /// <summary>Initializes a new instance of the <see cref="AIClient"/> class.</summary>
    /// <param name="chatClient">The underlying chat client pipeline.</param>
    /// <param name="templateStore">An optional prompt template store.</param>
    /// <param name="conversationStore">An optional conversation history store.</param>
    /// <param name="tools">Tools registered in dependency injection.</param>
    public AIClient(
        IChatClient chatClient,
        IPromptTemplateStore? templateStore = null,
        IConversationStore? conversationStore = null,
        IEnumerable<AITool>? tools = null)
    {
        ArgumentNullException.ThrowIfNull(chatClient);

        _context = new AIExecutionContext
        {
            ChatClient = chatClient,
            TemplateStore = templateStore,
            ConversationStore = conversationStore,
            RegisteredTools = tools is null ? [] : [.. tools],
        };
    }

    /// <inheritdoc />
    public IAIRequestBuilder Prompt(string prompt)
    {
        ArgumentException.ThrowIfNullOrEmpty(prompt);
        return new AIRequestBuilder(new AIRequestState { Context = _context, Prompt = prompt });
    }

    /// <inheritdoc />
    public IAIRequestBuilder Template(string templateName)
    {
        ArgumentException.ThrowIfNullOrEmpty(templateName);
        return new AIRequestBuilder(new AIRequestState { Context = _context, TemplateName = templateName });
    }
}
