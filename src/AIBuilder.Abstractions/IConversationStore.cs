namespace AIBuilder;

/// <summary>
/// A single message in a stored conversation.
/// </summary>
public sealed class ConversationMessage
{
    /// <summary>The role of the author (for example <c>user</c>, <c>assistant</c>, or <c>system</c>).</summary>
    public required string Role { get; init; }

    /// <summary>The message text.</summary>
    public required string Text { get; init; }

    /// <summary>When the message was created.</summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Stores conversation history so that requests can be made with prior context.
/// </summary>
public interface IConversationStore
{
    /// <summary>Gets the ordered history for a conversation.</summary>
    Task<IReadOnlyList<ConversationMessage>> GetAsync(string conversationId, CancellationToken cancellationToken = default);

    /// <summary>Appends a message to a conversation.</summary>
    Task AppendAsync(string conversationId, ConversationMessage message, CancellationToken cancellationToken = default);

    /// <summary>Clears all history for a conversation.</summary>
    Task ClearAsync(string conversationId, CancellationToken cancellationToken = default);
}
