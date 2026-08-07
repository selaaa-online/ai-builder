using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace AIBuilder.Core.Tests;

/// <summary>
/// A configurable in-memory <see cref="IChatClient"/> for tests.
/// </summary>
internal sealed class StubChatClient : IChatClient
{
    private readonly Func<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken, Task<ChatResponse>> _handler;

    public StubChatClient(
        Func<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken, Task<ChatResponse>> handler)
    {
        _handler = handler;
    }

    public int CallCount { get; private set; }

    public static StubChatClient Returning(string text) =>
        new((_, _, _) => Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, text))));

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        return _handler(messages, options, cancellationToken);
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ChatResponse response = await GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
        yield return new ChatResponseUpdate(ChatRole.Assistant, response.Text);
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
