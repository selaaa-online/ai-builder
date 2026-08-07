using System.Collections.Concurrent;

namespace AIBuilder.Memory;

/// <summary>
/// A thread-safe, in-memory <see cref="IConversationStore"/>.
/// </summary>
public sealed class InMemoryConversationStore : IConversationStore
{
    private readonly ConcurrentDictionary<string, List<ConversationMessage>> _conversations = new();

    /// <inheritdoc />
    public Task<IReadOnlyList<ConversationMessage>> GetAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(conversationId);

        if (!_conversations.TryGetValue(conversationId, out List<ConversationMessage>? messages))
        {
            return Task.FromResult<IReadOnlyList<ConversationMessage>>([]);
        }

        lock (messages)
        {
            return Task.FromResult<IReadOnlyList<ConversationMessage>>([.. messages]);
        }
    }

    /// <inheritdoc />
    public Task AppendAsync(string conversationId, ConversationMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(conversationId);
        ArgumentNullException.ThrowIfNull(message);

        List<ConversationMessage> messages = _conversations.GetOrAdd(conversationId, static _ => []);
        lock (messages)
        {
            messages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ClearAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(conversationId);
        _conversations.TryRemove(conversationId, out _);
        return Task.CompletedTask;
    }
}
