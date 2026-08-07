using Microsoft.Extensions.AI;

namespace AIBuilder.Middleware;

/// <summary>
/// Thrown when a request cannot obtain a rate-limit permit within the configured wait time.
/// </summary>
public sealed class RateLimitExceededException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="RateLimitExceededException"/> class.</summary>
    public RateLimitExceededException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// A <see cref="DelegatingChatClient"/> that throttles requests using a simple sliding window.
/// </summary>
public sealed class RateLimitingChatClient : DelegatingChatClient
{
    private readonly RateLimitOptions _options;
    private readonly Queue<long> _timestamps = new();
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>Initializes a new instance of the <see cref="RateLimitingChatClient"/> class.</summary>
    public RateLimitingChatClient(IChatClient innerClient, RateLimitOptions options)
        : base(innerClient)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.PermitLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "PermitLimit must be greater than zero.");
        }

        _options = options;
    }

    /// <inheritdoc />
    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await AcquireAsync(cancellationToken).ConfigureAwait(false);
        return await base.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await AcquireAsync(cancellationToken).ConfigureAwait(false);
        await foreach (ChatResponseUpdate update in base.GetStreamingResponseAsync(messages, options, cancellationToken).ConfigureAwait(false))
        {
            yield return update;
        }
    }

    private async Task AcquireAsync(CancellationToken cancellationToken)
    {
        long windowTicks = _options.Window.Ticks;
        long deadline = DateTime.UtcNow.Ticks + _options.MaxWait.Ticks;

        while (true)
        {
            TimeSpan wait;

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                long now = DateTime.UtcNow.Ticks;
                while (_timestamps.Count > 0 && now - _timestamps.Peek() >= windowTicks)
                {
                    _timestamps.Dequeue();
                }

                if (_timestamps.Count < _options.PermitLimit)
                {
                    _timestamps.Enqueue(now);
                    return;
                }

                long oldest = _timestamps.Peek();
                wait = TimeSpan.FromTicks(oldest + windowTicks - now);
            }
            finally
            {
                _gate.Release();
            }

            if (DateTime.UtcNow.Ticks + wait.Ticks > deadline)
            {
                throw new RateLimitExceededException(
                    $"Rate limit of {_options.PermitLimit} requests per {_options.Window} exceeded and MaxWait elapsed.");
            }

            await Task.Delay(wait, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _gate.Dispose();
        }

        base.Dispose(disposing);
    }
}
