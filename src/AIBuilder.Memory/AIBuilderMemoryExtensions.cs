using AIBuilder;
using AIBuilder.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Conversation memory registration extensions for <see cref="IAIBuilder"/>.
/// </summary>
public static class AIBuilderMemoryExtensions
{
    /// <summary>
    /// Registers an in-memory conversation store. Use it with <c>ai.Prompt(...).Chat("conversation-id")</c>.
    /// </summary>
    public static IAIBuilder UseInMemoryConversations(this IAIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.TryAddSingleton<IConversationStore, InMemoryConversationStore>();
        return builder;
    }
}
