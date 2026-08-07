using Microsoft.Extensions.AI;

namespace AIBuilder;

/// <summary>
/// Resolves request state (templates, conversation history, tools) into concrete
/// chat messages and options, and persists conversation turns afterwards.
/// </summary>
internal static class AIRequestExecutor
{
    public static async Task<AIPreparedRequest> PrepareAsync(AIRequestState state, CancellationToken cancellationToken)
    {
        string userText;
        string? systemPrompt = state.SystemPrompt;

        if (state.TemplateName is not null)
        {
            IPromptTemplateStore store = state.Context.TemplateStore
                ?? throw new InvalidOperationException(
                    "A prompt template was requested but no IPromptTemplateStore is registered. Call AddPromptTemplates(...).");

            PromptTemplate template = store.Get(state.TemplateName);
            userText = PromptTemplateRenderer.Render(template.Template, state.Variables);
            systemPrompt ??= template.SystemPrompt;
        }
        else
        {
            userText = state.Prompt
                ?? throw new InvalidOperationException("The request has no prompt or template.");
        }

        var messages = new List<ChatMessage>();

        if (!string.IsNullOrEmpty(systemPrompt))
        {
            messages.Add(new ChatMessage(ChatRole.System, systemPrompt));
        }

        if (state.ConversationId is not null && state.Context.ConversationStore is { } conversationStore)
        {
            IReadOnlyList<ConversationMessage> history =
                await conversationStore.GetAsync(state.ConversationId, cancellationToken).ConfigureAwait(false);

            foreach (ConversationMessage message in history)
            {
                messages.Add(new ChatMessage(new ChatRole(message.Role), message.Text));
            }
        }

        if (!string.IsNullOrEmpty(state.ContextText))
        {
            messages.Add(new ChatMessage(ChatRole.User, state.ContextText));
        }

        messages.Add(new ChatMessage(ChatRole.User, userText));

        IReadOnlyList<AITool> tools = state.UseRegisteredTools
            ? state.Context.RegisteredTools
            : state.Tools;

        if (tools.Count > 0)
        {
            state.Options.Tools = [.. tools];
        }

        return new AIPreparedRequest
        {
            Messages = messages,
            Options = state.Options,
            UserText = userText,
        };
    }

    public static async Task PersistAsync(
        AIRequestState state,
        string userText,
        string assistantText,
        CancellationToken cancellationToken)
    {
        if (state.ConversationId is null || state.Context.ConversationStore is not { } store)
        {
            return;
        }

        await store.AppendAsync(
            state.ConversationId,
            new ConversationMessage { Role = ChatRole.User.Value, Text = userText },
            cancellationToken).ConfigureAwait(false);

        await store.AppendAsync(
            state.ConversationId,
            new ConversationMessage { Role = ChatRole.Assistant.Value, Text = assistantText },
            cancellationToken).ConfigureAwait(false);
    }
}
